using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.StateMachine;
using VInspector;
using Obi;
using nn.hid;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
public class Player : MonoBehaviour
{
    private ImtStateMachine<Player, StateEvent> stateMachine;
    private class MyState : ImtStateMachine<Player, StateEvent>.State { }
    public enum StateEvent
    {
        Idle,
        Ground,
        Air,
        GrabbingRopeOnGround,
        GrabbingRopeOnAir,
    }
    [SerializeField] private float m_animSpeed = 1.5f;              // アニメーション再生速度設定
    [SerializeField] private float m_lookSmoother = 3.0f;           // a smoothing setting for camera motion
    [SerializeField] private bool useCurves = true;               // Mecanimでカーブ調整を使うか設定する
                                                                  // このスイッチが入っていないとカーブは使われない
    [SerializeField] private float useCurvesHeight = 0.5f;        // カーブ補正の有効高さ（地面をすり抜けやすい時には大きくする）

    // 以下キャラクターコントローラ用パラメタ
    // 前進速度
    [SerializeField] private float m_walkSpeed = 7.0f;
    // 後退速度
    [SerializeField] private float m_tarzanSpeed = 14.0f;
    //最高速度
    [SerializeField] private float m_maxSpeed = 20f;
    // 旋回速度
    [SerializeField] private float m_rotateSpeed = 1500.0f;
    // ジャンプ威力
    [SerializeField] private float m_jumpPower = 280f;

    [SerializeField] Vector3 m_playerForwardOn2d = Vector3.right;

    [SerializeField] private float m_grabTotalTime = 1f;

    //プレイヤーが持っているコンポーネント
    private Transform m_tf => transform;
    private CapsuleCollider m_capsuleCol;
    private Rigidbody m_rb;
    private PhysicMaterial m_physicMaterial;

    //キャラクターモデルが持っているコンポーネント
    private FullBodyBipedIK m_fullBodyBipedIK;
    private GrounderFBBIK m_grounderFBBIK;

    //アニメーション関係
    private Animator m_anim;
    private AnimatorStateInfo m_currentBaseState;
    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int MoveState = Animator.StringToHash("Base Layer.Blend Tree");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int fallState = Animator.StringToHash("Base Layer.Fall");
    static int restState = Animator.StringToHash("Base Layer.Rest");

    //移動に使用する
    private Vector3 m_moveDir;
    private Vector3 m_moveVec;
    private float m_moveSpeed;
    private Vector2 m_inputMoveDir;
    private Vector3 m_normal;
    private Vector3 m_oldPos;
    private Coroutine m_LegIKCor;

    //接地判定で使用する
    private bool m_isGround;
    public bool GetIsGround { get { return m_isGround; } }
    private bool m_oldIsGround;
    private float m_orgColHight;
    private Vector3 m_orgVectColCenter;
    private RaycastHit m_groundHit;

    //
    //カメラのTransform
    private Transform m_CameraTf;

    //ロープ発射関係
    private ObiSolver m_obiSolver;
    [SerializeField] private HookShot2 m_hookShot;
    private Transform m_hookShotTf;
    private ObiColliderBase m_obiCollider;
    private float m_lerpTForGrabbable = 0f;
    [SerializeField] private Grabbable m_grabbable;
    public Grabbable SetGrabbable
    {
        set { m_grabbable = value; }
    }

    public bool m_isGrabbing = false;
    [SerializeField] private Transform m_grabPoint;
    bool Tgr = false;
    [Button]
    void Test()
    {
        Tgr = !Tgr ? true : false;
        StateChangeSetupOnRopeGrab(Tgr);
    }

    #region 移動関係

    private void SetMoveDir()
    {
        float h;
        float v;
        m_inputMoveDir = Vector2.zero;
        if (MPFT_NTD_MMControlSystem.ms_instance != null)
        {
            h = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_X;
            v = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_Y;

            m_inputMoveDir = new Vector2(h, v);
        }
        else
        {
            h = Input.GetAxis("Horizontal");              // 入力デバイスの水平軸をhで定義
            v = Input.GetAxis("Vertical");                // 入力デバイスの垂直軸をvで定義

            m_inputMoveDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        //カメラが2Dか3Dか判定
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
    private void Move()
    {
        //アニメーション
        Vector3 Vec = m_tf.InverseTransformDirection(m_moveDir);
        //DebugPrint.Print(string.Format("AnimVec{0}", Vec));
        // Animator側で設定している"Speed"パラメタを渡す
        if (m_isGround)
        {
            m_anim.SetFloat("SpeedX", Vec.x);
            m_anim.SetFloat("SpeedY", Vec.z);
        }

        //移動入力があるか
        if (m_inputMoveDir.magnitude > 0)
        {
            //あれば移動用ベクトルを設定
            m_moveVec = m_moveDir;
            m_normal = m_groundHit.normal;
        }
        else
        {
            //なければ移動用ベクトルを0にする
            m_moveVec = Vector3.zero;
        }
        //移動方向に回転する処理
        if (m_moveDir.magnitude > 0)
        {
            m_tf.rotation = Quaternion.LookRotation(m_moveDir, Vector3.up);
        }
        else
        {
            m_tf.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(m_tf.forward, Vector3.up), Vector3.up);
        }

    }
    private void Tarzan()
    {
        //ターザン中は下降中のみプレイヤーの前方に力を与える
        //上昇中や入力がないときは力を与えない
        if (m_moveDir.magnitude > 0 &&
            m_rb.velocity.y < 0)
        {
            //下降中
            m_moveVec = m_moveDir;
        }
        else
        {
            m_moveVec = Vector3.zero;
        }
    }
    private void Jump()
    {
        if (m_currentBaseState.fullPathHash == MoveState && !m_anim.IsInTransition(0))
        {
            m_anim.SetBool("Jump", true);
        }
    }
    public void AddJumpPow()
    {
        m_rb.AddForce(Vector3.up * m_jumpPower, ForceMode.Impulse);
        m_anim.SetBool("Jump", false);
        Debug.Log("Jump");
        if (m_LegIKCor != null)
        {
            StopCoroutine(SetLegIKWeight());
        }
        m_LegIKCor = StartCoroutine(SetLegIKWeight());
    }
    private void RopeJump()
    {
        if (m_hookShot.GetCurrentAttachRb.isKinematic)
        {
            m_rb.AddForce(m_tf.forward * m_jumpPower, ForceMode.Impulse);
            Release();
        }
    }
    #endregion

    #region ロープ関係
    /// <summary>
    /// ロープ発射
    /// </summary>
    private void Launch()
    {
        m_hookShot.LaunchHook();

        StateChangeSetupOnRopeGrab(m_isGround);

        //StartCoroutine(RopeGrabSetupOnGround());
    }
    /// <summary>
    /// ロープ解除
    /// </summary>
    private void Detach()
    {
        m_hookShot.DetachHook();

        //プレイヤーのIKをリセット
        m_grabbable.SetParent(null);
        SetArmIKWeight(0);
        m_isGrabbing = false;
        m_hookShot.transform.position = m_grabPoint.position;
        //m_grabbable.SetParent(m_tf);

    }
    public void Release()
    {
        //    m_hookShot.SetGrabMesh(false);
        //    m_hookShot.ConnectCurrentObjToOtherObj(m_grabPoint.GetObiCol);
        //    //IKを解除し物理演算を開始
        //    SetIKWeight(0);
        //    m_grabPoint.SetParent(null);
        //    m_grabPoint.EnablePhysics();


        //    m_isGrabbing = false;
        //    Debug.Log("ロープを離しました");
    }

    public void GrabPointSetUp()
    {
        //m_grabPoint.SetUp();
        //SetIKWeight(1);
        //m_grabPoint.SetParent(m_tf);
        //Debug.Log("ロープを掴みました");
        //m_isGrabbing = true;
    }
    /// <summary>
    /// 地上でロープを掴んでいる状態の処理
    /// </summary>
    private void RopeGrabbingOnGround()
    {
        m_hookShot.RopeChangeLength();
    }
    /// <summary>
    /// 空中でロープを掴んでいる状態の処理
    /// </summary>
    private void RopeGrabbingOnAir()
    {
        m_hookShot.RopeChangeLength();
        m_anim.SetFloat("SpeedY", m_moveDir.z);

        //フックショットからロープが当たっているオブジェクトへの方向
        Vector3 ToAttachPointDir =
            (m_hookShot.GetCurrentAttachPos -
            m_hookShotTf.position
            ).normalized;

        Vector3 Dir;
        //移動入力がある時
        if (m_moveDir.magnitude > 0)
        {
            if (CameraChanger.ms_instance.m_is3DCamera)
            {
                Dir = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            }
            else
            {
                Dir = m_moveDir;
            }
        }
        //移動入力がない時
        else
        {
            if (CameraChanger.ms_instance.m_is3DCamera)
            {
                Dir = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            }
            else
            {
                Dir = m_tf.forward;
            }
            //Dir = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
           
        }
        //ロープの向きを基準に回転する
        m_tf.rotation =
            Quaternion.RotateTowards(
            m_tf.rotation,
            Quaternion.FromToRotation(
                Vector3.up, ToAttachPointDir) *
                Quaternion.LookRotation(Dir),
            m_rotateSpeed);

    }
    #endregion

    /// <summary>
    /// 地上と空中状態が変化した瞬間に行う処理
    /// </summary>
    /// <param name="IsGround"></param>
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
    /// <summary>
    /// ロープを掴んだ状態で地上と空中状態が変化した瞬間に行う処理
    /// </summary>
    /// <param name="isGround"></param>
    public void StateChangeSetupOnRopeGrab(bool isGround)
    {
        //m_grabbable.SetParent(null);
        Vector3 GrabPos;
        if (isGround)
        {
            //m_hookShot.SetIsKinematic = true;

            //手元
            m_hookShotTf.position = m_grabPoint.position;
            //GrabPos = m_grabPoint.position;
        }
        else
        {
            //m_hookShot.SetIsKinematic = false;

            //m_hookShotTf.position = m_tf.position + m_tf.up * m_orgColHight;
            m_hookShotTf.localPosition = m_orgVectColCenter * 2;
            // = m_tf.position + m_tf.up * m_orgColHight;
        }
        //yield return null;


        //m_grabbable.SetParent(m_tf);
        //m_hookShot.ConnectToSelf(m_hookShot.GetCurrnetAttachObiCol);
        //m_hookShot.SetIsKinematic = true;

        GrabPos = m_hookShotTf.position;

        m_grabbable.SetArmIKTarget(ref m_fullBodyBipedIK);

        //プレイヤーのIKをセット
        SetArmIKWeight(1);


        //yield return new WaitForSeconds(0.25f);

        m_isGrabbing = true;
        m_hookShot.ConnectToPlayer(m_obiCollider, m_tf.InverseTransformPoint(GrabPos));





        //yield return null;

        //m_hookShot.SetIsKinematic = isGround;


    }
    private void EnablePhysics()
    {
        m_hookShot.SetIsKinematic = true;
        m_hookShot.ConnectToSelf(m_hookShot.GetCurrentAttachObiCol);
    }
    /// <summary>
    /// 腕のIKのWeghtを設定
    /// </summary>
    /// <param name="weight"></param>
    private void SetArmIKWeight(float weight)
    {
        m_fullBodyBipedIK.solver.leftHandEffector.positionWeight = weight;
        m_fullBodyBipedIK.solver.leftHandEffector.rotationWeight = weight;
        m_fullBodyBipedIK.solver.rightHandEffector.positionWeight = weight;
        m_fullBodyBipedIK.solver.rightHandEffector.rotationWeight = weight;

        //Weghtが0の時に飾りのロープの描画をしない
        if (weight <= 0)
        {
            m_hookShot.SetGrabRopeMesh = false;
        }
        else
        {
            m_hookShot.SetGrabRopeMesh = true;
        }
    }
    private IEnumerator SetLegIKWeight()
    {
        // ジャンプ開始して一定時間で
        // IK処理のウェイトを1から0に変更して、
        // IK処理の影響を徐々に受けないようにする
        float elapsed = 0f;
        float time = 0.25f;
        while (elapsed < time)
        {
            float alpha = elapsed / time;
            m_grounderFBBIK.weight = 1f - alpha;
            yield return null;

            elapsed += Time.deltaTime;
        }
        m_grounderFBBIK.weight = 0;
        // ジャンプによる上昇中か、
        // 地面に接地していない間は、地面とのIK処理を切ったままにする
        while (m_rb.velocity.y > 0f || !m_isGround)
        {
            yield return null;
        }
        // 一定時間で地面とのIK処理のウェイトを元に戻す
        elapsed = 0;
        time = 0.25f;

        while (elapsed < time)
        {
            float alpha = elapsed / time;
            m_grounderFBBIK.weight = alpha;
            yield return null;

            elapsed += Time.deltaTime;
        }
        m_grounderFBBIK.weight = 1f;
        //コルーチンの終了
        m_LegIKCor = null;
    }
    private bool FootCollider()
    {
        return (Physics.SphereCast(m_tf.position + m_capsuleCol.center, m_capsuleCol.radius, -m_tf.up, out m_groundHit, m_capsuleCol.height / 1.5f, 1 << LayerMask.NameToLayer("Ground")));
    }
    private void Awake()
    {
        m_capsuleCol = GetComponent<CapsuleCollider>();
        m_orgColHight = m_capsuleCol.height;
        m_orgVectColCenter = m_capsuleCol.center;
        m_obiCollider = GetComponent<ObiColliderBase>();

        m_rb = GetComponent<Rigidbody>();
        m_physicMaterial = new PhysicMaterial();

        m_physicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
        m_capsuleCol.material = m_physicMaterial;
        m_isGround = m_oldIsGround = true;

        m_CameraTf = Camera.main.transform;
        m_anim = GetComponentInChildren<Animator>();
        //m_hookShot = GetComponentInChildren<HookShot2>();
        m_fullBodyBipedIK = GetComponentInChildren<FullBodyBipedIK>();
        m_grounderFBBIK = GetComponentInChildren<GrounderFBBIK>();
        m_obiSolver = FindObjectOfType<ObiSolver>();

        //var HookObj = Instantiate(m_hookShot, m_tf);
        m_hookShot = GetComponentInChildren<HookShot2>();
        m_hookShotTf = m_hookShot.transform;
        m_hookShotTf.position = m_grabPoint.position;
        m_grabbable = GetComponentInChildren<Grabbable>();
        m_hookShotTf.position = m_grabPoint.position;

        m_grabbable.SetUp();
        //疑似的な親子関係を結ぶ
        //m_grabbable.SetParent(m_tf);



        stateMachine = new ImtStateMachine<Player, StateEvent>(this);

        stateMachine.AddTransition<GroundState, AirState>(StateEvent.Air);
        stateMachine.AddTransition<GroundState, GrabbingRopeOnGround>(StateEvent.GrabbingRopeOnGround);
        stateMachine.AddTransition<AirState, GroundState>(StateEvent.Ground);
        stateMachine.AddTransition<AirState, GrabbingRopeOnAir>(StateEvent.GrabbingRopeOnAir);
        stateMachine.AddTransition<GrabbingRopeOnGround, GroundState>(StateEvent.Ground);
        stateMachine.AddTransition<GrabbingRopeOnGround, GrabbingRopeOnAir>(StateEvent.GrabbingRopeOnAir);
        stateMachine.AddTransition<GrabbingRopeOnAir, AirState>(StateEvent.Air);
        stateMachine.AddTransition<GrabbingRopeOnAir, GrabbingRopeOnGround>(StateEvent.GrabbingRopeOnGround);

        stateMachine.SetStartState<GroundState>();
    }

    // Start is called before the first frame update
    void Start()
    {
        m_currentBaseState = m_anim.GetCurrentAnimatorStateInfo(0);





    }

    // Update is called once per frame
    void Update()
    {
        m_isGround = FootCollider();
        SetMoveDir();

        DebugPrint.Print(string.Format("{0}", m_moveVec));
        DebugPrint.Print(string.Format("{0}", m_moveSpeed));

        m_anim.SetBool("Ground", m_isGround);

        //m_hookShot.HookShooting();
        DebugPrint.Print(string.Format("CurrentStateName:{0}", stateMachine.CurrentStateName));
        stateMachine.Update();
    }
    private void FixedUpdate()
    {

        m_rb.AddForce(Vector3.ProjectOnPlane(m_moveVec, m_normal) * m_moveSpeed, ForceMode.Force);

    }
    private class GroundState : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();
            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(true);

            Context.SetArmIKWeight(0);
            Context.m_moveSpeed = Context.m_walkSpeed;

            Context.m_anim.SetBool("Jump", false);
        }
        protected internal override void Update()
        {
            base.Update();
            //状態遷移
            if (!Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.Air);
            }

            if (Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnGround);
            }

            //DebugPrint.Print(string.Format("Test"));

            //移動処理
            Context.Move();

            //ロープ発射
            if (Input.GetMouseButtonDown(1))
            {
                Context.Launch();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
            {
                Context.Launch();
            }



            //ジャンプ
            if (Input.GetButtonDown("Jump"))
            {
                Context.Jump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null)
            {
                if (MPFT_NTD_MMControlSystem.ms_instance.npadState.GetButtonDown(NpadButton.A))
                {
                    Context.Jump();
                }
            }
        }


    }
    private class AirState : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();
            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(false);
        }
        protected internal override void Update()
        {
            base.Update();
            if (Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.Ground);
            }
            if (Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnAir);
            }



            Context.Move();
            if (Input.GetMouseButtonDown(1))
            {
                Context.Launch();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
            {
                Context.Launch();
            }
        }
        protected internal override void Exit()
        {
            base.Exit();
        }
    }
    private class GrabbingRopeOnGround : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();

            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(true);
            //Context.m_anim.SetBool("RopeGrab", true);
            //Context.StartCoroutine(Context.StateChangeSetup(true));

            Context.m_moveSpeed = Context.m_walkSpeed;
        }
        protected internal override void Update()
        {
            base.Update();
            if (!Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnAir);
                Context.StateChangeSetupOnRopeGrab(false);
            }
            if (!Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.Ground);
            }
            Context.Move();
            Context.RopeGrabbingOnGround();

            //ジャンプ
            if (Input.GetButtonDown("Jump"))
            {
                Context.Jump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null)
            {
                if (MPFT_NTD_MMControlSystem.ms_instance.npadState.GetButtonDown(NpadButton.A))
                {
                    Context.Jump();
                }
            }
            //ロープを離す
            if (Input.GetKeyDown(KeyCode.V))
            {
                Context.Release();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
            {
                Context.Jump();
            }
            //ロープを解除
            if (Input.GetMouseButtonDown(1))
            {
                Context.Detach();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
            {
                Context.Detach();
            }
        }
        protected internal override void Exit()
        {
            base.Exit();
            //Context.m_anim.SetBool("RopeGrab", false);

        }
    }
    private class GrabbingRopeOnAir : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();
            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(false);
            Context.m_anim.SetBool("RopeGrab", true);
            //Context.StartCoroutine(Context.StateChangeSetup(false));

            Context.m_moveSpeed = Context.m_tarzanSpeed;
        }
        protected internal override void Update()
        {


            base.Update();
            if (Context.m_isGround)
            {

                //Context.StartCoroutine(Context.StateChangeSetupOnRopeGrab(true));
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnGround);
                Context.StateChangeSetupOnRopeGrab(true);
            }
            if (!Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.Air);
            }
            Context.Tarzan();
            Context.RopeGrabbingOnAir();

            if (Input.GetButtonDown("Jump"))
            {
                Context.RopeJump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null)
            {
                if (MPFT_NTD_MMControlSystem.ms_instance.npadState.GetButtonDown(NpadButton.A))
                {
                    Context.Jump();
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                Context.Detach();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
            {
                Context.Detach();
            }
        }
        protected internal override void Exit()
        {
            base.Exit();
            Context.m_anim.SetBool("RopeGrab", false);

            //Context.EnablePhysics();
        }
    }
}
