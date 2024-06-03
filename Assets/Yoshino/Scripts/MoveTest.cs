using Obi;
using RootMotion.FinalIK;
using System.Collections;
using UnityEngine;
using VInspector;

public class MoveTest : MonoBehaviour
{
    [SerializeField] private float m_animSpeed = 1.5f;              // アニメーション再生速度設定
    [SerializeField] private float m_lookSmoother = 3.0f;           // a smoothing setting for camera motion
    [SerializeField] private bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
                                                                  // このスイッチが入っていないとカーブは使われない
    [SerializeField] private float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

    // 以下キャラクターコントローラ用パラメタ
    // 前進速度
    [SerializeField] private float m_forwardSpeed = 7.0f;
    // 後退速度
    [SerializeField] private float m_backwardSpeed = 2.0f;
    // 旋回速度
    [SerializeField] private float m_rotateSpeed = 1500.0f;
    // ジャンプ威力
    [SerializeField] private float m_jumpPower = 3.0f;

    [SerializeField] Vector3 m_playerForwardOn2d = Vector3.right;

    [SerializeField] private float m_grabTotalTime = 1f;

    // キャラクターコントローラ（カプセルコライダ）の参照
    private CapsuleCollider m_col;
    private Rigidbody m_rb;
    private Transform m_tf;
    private PhysicMaterial m_physicMaterial;
    //カメラのTransform
    private Transform m_CameraTf;
    private bool m_isGround;
    private bool m_oldIsGround;
    public bool GetIsGround
    {
        get { return m_isGround; }
    }

    private FullBodyBipedIK m_fullBodyBipedIK;
    private float m_ikArmWeight = 0;

    // キャラクターコントローラ（カプセルコライダ）の移動量
    private Vector3 m_moveDir;
    private Vector3 m_oldPos;
    private bool m_isInputJump;
    // CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
    private float m_orgColHight;
    private Vector3 m_orgVectColCenter;
    private RaycastHit m_groundHit;
    private Animator m_anim;                          // キャラにアタッチされるアニメーターへの参照
    private AnimatorStateInfo m_currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照

    // アニメーター各ステートへの参照
    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int MoveState = Animator.StringToHash("Base Layer.Blend Tree");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int fallState = Animator.StringToHash("Base Layer.Fall");
    static int restState = Animator.StringToHash("Base Layer.Rest");
    private HookShot m_hookShot;
    private IKTarget m_ikTarget;
    private float m_lerpTGrabPoint = 0f;
    private GrabPoint m_grabPoint;
    public GrabPoint SetGrabPoint
    {
        set { m_grabPoint = value; }
    }

    public bool m_isGrabbing = false;
    // 初期化
    void Start()
    {
        // Animatorコンポーネントを取得する
        m_anim = GetComponentInChildren<Animator>();
        // CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
        m_col = GetComponent<CapsuleCollider>();
        m_rb = GetComponent<Rigidbody>();
        m_physicMaterial = new PhysicMaterial();
        LandingAndJumpSetUp(true);
        m_physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        m_col.material = m_physicMaterial;

        //メインカメラを取得する
        // CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
        m_orgColHight = m_col.height;
        m_orgVectColCenter = m_col.center;
        m_tf = transform;
        m_CameraTf = Camera.main.transform;
        m_hookShot = GetComponentInChildren<HookShot>();
        m_ikTarget = GetComponentInChildren<IKTarget>();
        m_fullBodyBipedIK = GetComponentInChildren<FullBodyBipedIK>();
        //contactEventDispatcher.onContactEnter
        SetIKWeight(0);
    }

    private void Update()
    {
        m_anim.speed = m_animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
        m_currentBaseState = m_anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
        DebugPrint.Print(string.Format("Ground{0}", m_isGround));
        m_anim.SetBool("Ground", m_isGround);
        SetMoveDir();
        //空中では歩行アニメーションをしないようにする処理
        if (m_isGround)
        {
            Vector3 Vec = m_tf.InverseTransformDirection(m_moveDir);
            //DebugPrint.Print(string.Format("AnimVec{0}", Vec));
            // Animator側で設定している"Speed"パラメタを渡す
            m_anim.SetFloat("SpeedX", Vec.x);
            m_anim.SetFloat("SpeedY", Vec.z);
        }
        else
        {
            m_anim.SetFloat("SpeedX", 0);
            m_anim.SetFloat("SpeedY", 0);
        }

        //DebugPrint.Print(string.Format("ISMove{0}", m_currentBaseState.fullPathHash== locoState));
        //ジャンプ
        if (Input.GetButtonDown("Jump"))
        {
            if (m_currentBaseState.fullPathHash != jumpState || m_currentBaseState.fullPathHash != fallState)
            {
                m_isInputJump = true;

            }
        }
        //if (MPFT_NTD_MMControlSystem.ms_instance != null&& MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
        //{
        //    m_isInputJump = true;
        //}
        if (m_currentBaseState.fullPathHash == jumpState)
        {
            if (!m_anim.IsInTransition(0))
            {
                m_anim.SetBool("Jump", false);
            }
        }
        m_hookShot.HookShooting();
        //DebugPrint.Print(string.Format("{0}", m_ikArmWeight));

        if (!m_isGrabbing)
        {
            Idle();
        }
        else
        {
            RopeGrabbing();
        }
    }
    // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
    void FixedUpdate()
    {
        ForceMode Mode = ForceMode.Acceleration;
        float Speed = m_forwardSpeed;
        m_isGround = FootCollider();
        //接地した瞬間、または宙に浮いた瞬間の処理
        if (m_isGround != m_oldIsGround)
        {
            LandingAndJumpSetUp(m_isGround);
        }
        //ターザン風の移動処理
        //フックを発射中かつ、空中で、上昇中は移動速度を0にする
        if (m_isGrabbing && !m_isGround && (m_oldPos.y - m_tf.position.y) <= 0f)
        {
            Speed = 0f;
        }

        //キャラクターを移動させる
        Vector3 Normal;
        if (m_isGround)
        {
            Normal = m_groundHit.normal;

            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            //入力がないときは、
            if (h == 0 && v == 0)
            {
                m_rb.velocity = Vector3.MoveTowards(m_rb.velocity, Vector3.zero, 0.5f);
            }
            else
            {
                m_rb.drag = 1;
            }

        }
        else
        {
            Normal = Vector3.up;
        }
        m_rb.AddForce(Vector3.ProjectOnPlane(m_moveDir, Normal) * Speed, Mode);

        //
        if (!CameraChanger.ms_instance.m_is3DCamera)
        {
            m_tf.position = new Vector3(m_tf.position.x, m_tf.position.y, 0f);
        }
        m_oldPos = m_tf.position;
        m_oldIsGround = m_isGround;
    }
    private void SetMoveDir()
    {
        float h = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
        float v = Input.GetAxis("Vertical");                // 入力デバイスの垂直軸をvで定義
                                                            //if (MPFT_NTD_MMControlSystem.ms_instance != null)
                                                            //{
                                                            //    h = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_X;
                                                            //    v = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_Y;
                                                            //}

        if (CameraChanger.ms_instance.m_is3DCamera)
        {
            //3Dの場合はカメラに依存する
            Vector3 Forward = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            m_moveDir = (Forward * v + m_CameraTf.right * h);
        }
        else
        {
            //2Dの場合は左右の入力のみを移動に使用する
            m_moveDir = (m_playerForwardOn2d * h);

        }

        DebugPrint.Print(string.Format("MoveVec{0}", m_moveDir));
    }
    public bool LerpGrabPoint()
    {
        //
        m_lerpTGrabPoint += Time.deltaTime / m_grabTotalTime;

        m_grabPoint.transform.position = Vector3.Lerp(m_grabPoint.transform.position, m_hookShot.transform.position, m_lerpTGrabPoint);

        m_ikTarget.Move(m_grabPoint.transform.position);
        return m_lerpTGrabPoint >= 1;
    }
    public void SetIKWeight(float weight)
    {
        m_ikArmWeight = weight;
        m_fullBodyBipedIK.solver.leftHandEffector.positionWeight = m_ikArmWeight;
        m_fullBodyBipedIK.solver.leftHandEffector.rotationWeight = m_ikArmWeight;
        m_fullBodyBipedIK.solver.rightHandEffector.positionWeight = m_ikArmWeight;
        m_fullBodyBipedIK.solver.rightHandEffector.rotationWeight = m_ikArmWeight;
    }
    /// <summary>
    /// ロープを離す処理
    /// </summary>
    public void Release()
    {
        m_hookShot.SetGrabMesh(false);
        m_hookShot.ConnectCurrentObjToOtherObj(m_grabPoint.GetObiCol);
        //IKを解除し物理演算を開始
        SetIKWeight(0);
        m_grabPoint.SetParent(null);
        m_grabPoint.EnablePhysics();


        m_isGrabbing = false;
        Debug.Log("ロープを離しました");
    }

    public IEnumerator Grab()
    {
        m_isGrabbing = true;
        Debug.Log("ロープを掴みました");
        m_grabPoint.DisableCollider();
        m_lerpTGrabPoint = 0f;
        //m_hookShot.DisabledCollition();
        SetIKWeight(1);
        m_hookShot.SetGrabMesh(true);
        yield return new WaitUntil(() => LerpGrabPoint());
        m_hookShot.PlayerGrabs();
        m_grabPoint.SetParent(m_tf);
        //m_hookShot.GrabRope();
    }
    public void GrabPointSetUp()
    {
        m_grabPoint.SetUp();
        SetIKWeight(1);
        m_grabPoint.SetParent(m_tf);
        Debug.Log("ロープを掴みました");
        m_isGrabbing = true;
    }


    /// <summary>
    ///　通常状態
    /// </summary>
    private void Idle()
    {
        if (m_isGrabbing) return;
        if (m_grabPoint != null)
        {
            float dist = Vector3.Distance(m_tf.position, m_grabPoint.transform.position);
            float length = 2f;
            if (Input.GetKeyDown(KeyCode.V) && dist <= length)
            {
                StartCoroutine(Grab());
            }
        }
        //ジャンプ処理
        if (m_isInputJump && m_currentBaseState.fullPathHash == MoveState)
        {
            m_anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
            m_isInputJump = false;
        }
        //移動中は移動方向へ向く|| Mathf.Abs((m_oldPos - m_tf.position).magnitude) > 0f
        if (m_isGround)
        {

            if (m_moveDir.magnitude > 0)
            {
                m_tf.rotation = Quaternion.LookRotation(m_moveDir, Vector3.up);
            }
            else
            {
                m_tf.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_tf.forward, Vector3.up), Vector3.up);
            }

        }
    }
    /// <summary>
    /// ロープを掴んだ状態
    /// </summary>
    private void RopeGrabbing()
    {
        if (!m_isGrabbing) return;
        if (Input.GetKeyDown(KeyCode.V))
        {
            Release();
        }
        //if (MPFT_NTD_MMControlSystem.ms_instance != null&& MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
        //{
        //    Release();
        //}
        if (Input.GetKeyDown(KeyCode.E))
        {
            Release();
            m_hookShot.ConnectCurrentObjToOtherObj(m_hookShot.GetAttachmentTargetObiCol);
        }
        //if (MPFT_NTD_MMControlSystem.ms_instance != null && || MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TL)
        //{
        //    Release();
        //    m_hookShot.ConnectCurrentObjToOtherObj(m_hookShot.GetAttachmentTargetObiCol);
        //}
        //ロープジャンプ処理
        if (m_isInputJump && m_isGrabbing)
        {
            if (m_isGround)
            {
                m_anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
            }
            else if (m_hookShot.GetCurrnetAttachRb.isKinematic)
            {
                m_rb.AddForce(m_tf.forward * m_jumpPower, ForceMode.Impulse);
                Release();
            }
            m_isInputJump = false;
        }
        //m_ikTarget.Move(m_grabPoint.transform.position);
        m_hookShot.RopeChangeLength();
        //HookShotの方向に向き続ける
        Vector3 ToGrabPointDir = (m_grabPoint.transform.position - m_tf.position).normalized;
        Vector3 ToAttachPointDir = (m_hookShot.GetCurrnetAttachTf.position - m_tf.position).normalized;
        Debug.DrawRay(m_tf.position, ToGrabPointDir * 100);
        if (m_isGround)
        {
            ToGrabPointDir = Vector3.ProjectOnPlane(ToGrabPointDir, Vector3.up);
            m_tf.rotation = Quaternion.LookRotation(ToGrabPointDir, Vector3.up);
        }
        else if (m_hookShot.GetCurrnetAttachRb.isKinematic)
        {
            //ToAttachPointDir = Vector3.ProjectOnPlane(ToGrabPointDir, Vector3.up);
            //m_tf.rotation = Quaternion.LookRotation(Dir, Vector3.up);
            m_moveDir = Vector3.ProjectOnPlane(m_moveDir, ToAttachPointDir);

            m_tf.rotation = Quaternion.RotateTowards(m_tf.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(m_moveDir, ToAttachPointDir)), m_rotateSpeed * Time.fixedDeltaTime);
            //Quaternion.LookRotation(Dir, Vector3.up) *
            //m_tf.rotation = Quaternion.LookRotation(Dir) * Quaternion.Euler(m_tf.rotation.x + 90, m_tf.rotation.y, m_tf.rotation.z);

        }

    }

    void OnGUI()
    {
        //GUI.Box(new Rect(Screen.width - 260, 10, 250, 150), "Interaction");
        //GUI.Label(new Rect(Screen.width - 245, 30, 250, 30), "Up/Down Arrow : Go Forwald/Go Back");
        //GUI.Label(new Rect(Screen.width - 245, 50, 250, 30), "Left/Right Arrow : Turn Left/Turn Right");
        //GUI.Label(new Rect(Screen.width - 245, 70, 250, 30), "Hit Space key while Running : Jump");
        //GUI.Label(new Rect(Screen.width - 245, 90, 250, 30), "Hit Spase key while Stopping : Rest");
        //GUI.Label(new Rect(Screen.width - 245, 110, 250, 30), "Left Control : Front Camera");
        //GUI.Label(new Rect(Screen.width - 245, 130, 250, 30), "Alt : LookAt Camera");
    }

    // キャラクターのコライダーサイズのリセット関数
    void resetCollider()
    {
        // コンポーネントのHeight、Centerの初期値を戻す
        m_col.height = m_orgColHight;
        m_col.center = m_orgVectColCenter;
    }
    private bool FootCollider()
    {
        return (Physics.SphereCast(m_tf.position + m_col.center, m_col.radius, -m_tf.up, out m_groundHit, m_col.height / 1.5f, 1 << LayerMask.NameToLayer("Ground")));
    }
    private void LandingAndJumpSetUp(bool IsGround)
    {
        if (IsGround)
        {
            //接地中は摩擦力を１に
            m_physicMaterial.dynamicFriction = 1;
            m_physicMaterial.staticFriction = 1;
            //重力を無効化
            m_rb.useGravity = false;
        }
        else
        {

            //空中では摩擦力を0にして、壁に当たっているとき空中で留まることを防ぐ
            m_physicMaterial.dynamicFriction = 0;
            m_physicMaterial.staticFriction = 0;
            //重力を有効化
            m_rb.useGravity = true;
        }
    }
    public void Jump()
    {
        m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
    }
}

