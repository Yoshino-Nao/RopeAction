using Obi;
using RootMotion.FinalIK;
using UnityEngine;

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
    [SerializeField] private float m_rotateSpeed = 2.0f;
    // ジャンプ威力
    [SerializeField] private float m_jumpPower = 3.0f;

    [SerializeField] Vector3 m_playerForwardOn2d = Vector3.right;
    // キャラクターコントローラ（カプセルコライダ）の参照
    private CapsuleCollider m_col;
    private Rigidbody m_rb;
    private Transform m_tf;
    private Transform m_CameraTf;

    private FullBodyBipedIK m_fullBodyBipedIK;
    private float m_ikArmWeght = 0;

    // キャラクターコントローラ（カプセルコライダ）の移動量
    private Vector3 m_moveVec;
    private bool m_isInputJump;
    // CapsuleColliderで設定されているコライダのHeiht、Centerの初期値を収める変数
    private float m_orgColHight;
    private Vector3 m_orgVectColCenter;
    private Animator m_anim;                          // キャラにアタッチされるアニメーターへの参照
    private AnimatorStateInfo m_currentBaseState;         // base layerで使われる、アニメーターの現在の状態の参照


    // アニメーター各ステートへの参照
    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locoState = Animator.StringToHash("Base Layer.Locomotion");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int restState = Animator.StringToHash("Base Layer.Rest");

    private CameraChanger m_cameraChanger;
    HookShot m_hookShot;
    // 初期化
    void Start()
    {
        // Animatorコンポーネントを取得する
        m_anim = GetComponentInChildren<Animator>();
        // CapsuleColliderコンポーネントを取得する（カプセル型コリジョン）
        m_col = GetComponent<CapsuleCollider>();
        m_rb = GetComponent<Rigidbody>();
        //メインカメラを取得する
        // CapsuleColliderコンポーネントのHeight、Centerの初期値を保存する
        m_orgColHight = m_col.height;
        m_orgVectColCenter = m_col.center;
        m_tf = transform;
        m_CameraTf = Camera.main.transform;
        m_hookShot = GetComponentInChildren<HookShot>();
        m_fullBodyBipedIK = GetComponentInChildren<FullBodyBipedIK>();
        //contactEventDispatcher.onContactEnter
        m_cameraChanger = GetComponent<CameraChanger>();
    }

    private void Update()
    {
        m_anim.speed = m_animSpeed;                             // Animatorのモーション再生速度に animSpeedを設定する
        m_currentBaseState = m_anim.GetCurrentAnimatorStateInfo(0); // 参照用のステート変数にBase Layer (0)の現在のステートを設定する
        m_rb.useGravity = true;//ジャンプ中に重力を切るので、それ以外は重力の影響を受けるようにする
        float h = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
        float v = Input.GetAxis("Vertical");                // 入力デバイスの垂直軸をvで定義

        DebugPrint.Print(string.Format("X{0}", h));
        DebugPrint.Print(string.Format("Y{0}", v));

        m_isInputJump = Input.GetButtonDown("Jump");
        m_anim.SetBool("Jump", false);
        if (m_cameraChanger.m_is3DCamera)
        {
            //3Dの場合はカメラに依存する
            Vector3 Forward = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            m_moveVec = NormalizedEx((Forward * v + m_CameraTf.right * h));

            DebugPrint.Print(string.Format("3D"));
        }
        else
        {
            v = 0f;
            //2Dの場合は左右の入力のみを移動に使用する
            m_moveVec = (m_playerForwardOn2d * h).normalized;
            DebugPrint.Print(string.Format("2D"));
        }
        DebugPrint.Print(string.Format("MoveVec{0}", m_moveVec));
        // 以下、キャラクターの移動処理
        //空中では歩行をしないようにする処理
        if (FootCollider())
        {
            Vector2 Vec = new Vector2(h, v).normalized;
            // Animator側で設定している"Speed"パラメタを渡す
            m_anim.SetFloat("SpeedX", m_tf.forward.x);
            m_anim.SetFloat("SpeedY", m_tf.forward.z);
            //DebugPrint.Print(string.Format("AnimX{0}", m_anim.GetFloat("SpeedX")));
            //DebugPrint.Print(string.Format("AnimY{0}", m_anim.GetFloat("SpeedY")));

        }
        else
        {
            m_anim.SetFloat("SpeedX", 0);
            m_anim.SetFloat("SpeedY", 0);
        }
        float WeghtTarget;
        //フックを発射中でない時
        if (!m_hookShot.GetisLoaded)
        {
            //移動中は移動方向へ向く
            if (m_moveVec.magnitude >= 0.1)
            {
                m_tf.rotation = Quaternion.LookRotation(m_moveVec, Vector3.up);

            }
            WeghtTarget = 0;
        }
        //フックを発射中
        else
        {
            Vector3 Dir = (m_hookShot.GetAttachmentObj.transform.position - m_tf.position).normalized;
            Dir = Vector3.ProjectOnPlane(Dir, Vector3.up);
            m_tf.rotation = Quaternion.LookRotation(Dir, Vector3.up);
            WeghtTarget = 1;
        }
        m_ikArmWeght = Mathf.MoveTowards(m_ikArmWeght, WeghtTarget, Time.deltaTime * 2);
        m_fullBodyBipedIK.solver.leftHandEffector.positionWeight = m_ikArmWeght;
        m_fullBodyBipedIK.solver.rightHandEffector.positionWeight = m_ikArmWeght;
        //ジャンプ
        if (m_isInputJump)
        {   // スペースキーを入力したら

            //アニメーションのステートがLocomotionの最中のみジャンプできる
            //if (m_currentBaseState.fullPathHash == locoState && !m_hookShot.GetisLoaded)
            {
                //ステート遷移中でなかったらジャンプできる
                if (!m_anim.IsInTransition(0))
                {
                    m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
                    m_anim.SetBool("Jump", true);     // Animatorにジャンプに切り替えるフラグを送る
                }
            }
        }



        //m_tf.localPosition += m_moveVec * forwardSpeed * Time.fixedDeltaTime;

        // 以下、Animatorの各ステート中での処理
        // Locomotion中
        // 現在のベースレイヤーがlocoStateの時
        if (m_currentBaseState.fullPathHash == locoState)
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (useCurves)
            {
                resetCollider();
            }
        }
    }
    private Vector3 NormalizedEx(Vector3 vec)
    {
        float len = Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y + vec.z * vec.z);
        if(len <= 0) 
        {
            return Vector3.zero;
        }
        return new Vector3(vec.x / len, vec.y / len, vec.z / len);
    }
    // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
    void FixedUpdate()
    {


        //キャラクターを移動させる
        m_rb.AddForce(m_moveVec * m_forwardSpeed, ForceMode.Acceleration);











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
        RaycastHit hit;
        return (Physics.SphereCast(m_tf.position + m_col.center, m_col.radius, -Vector3.up, out hit, m_col.height / 2));
    }

}

