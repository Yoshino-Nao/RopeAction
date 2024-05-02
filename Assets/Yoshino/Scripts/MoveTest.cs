using Obi;
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
    [SerializeField] private RopeCollisionDetector m_collisionDetector;
    private ObiContactEventDispatcher contactEventDispatcher;
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
        contactEventDispatcher = FindObjectOfType<ObiContactEventDispatcher>();

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
        m_isInputJump = Input.GetButtonDown("Jump");

        if (m_cameraChanger.m_is3DCamera)
        {
            //3Dの場合はカメラに依存する
            Vector3 Forward = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            m_moveVec = (Forward * v + m_CameraTf.right * h).normalized;
            DebugPrint.Print(string.Format("3D"));
        }
        else
        {
            v = 0f;
            //2Dの場合は左右の入力のみを移動に使用する
            m_moveVec = (m_playerForwardOn2d * h).normalized;
            DebugPrint.Print(string.Format("2D"));
        }

        // 以下、キャラクターの移動処理
        //空中では歩行をしないようにする処理
        if (FootCollider())
        {
            // Animator側で設定している"Speed"パラメタを渡す
            m_anim.SetFloat("Speed", Mathf.Pow(h, 2) + Mathf.Pow(v, 2));
        }
        else
        {
            m_anim.SetFloat("Speed", 0);
        }
        //移動中は移動方向へ向く
        if (m_moveVec.magnitude >= 0.1)
        {
            m_tf.rotation = Quaternion.LookRotation(m_moveVec, Vector3.up);
        }
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
        // JUMP中の処理
        // 現在のベースレイヤーがjumpStateの時
        else if (m_currentBaseState.fullPathHash == jumpState)
        {
            //cameraObject.SendMessage("setCameraPositionJumpView");  // ジャンプ中のカメラに変更
            // ステートがトランジション中でない場合
            if (!m_anim.IsInTransition(0))
            {

                // 以下、カーブ調整をする場合の処理
                if (useCurves)
                {
                    // 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
                    // JumpHeight:JUMP00でのジャンプの高さ（0〜1）
                    // GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
                    float jumpHeight = m_anim.GetFloat("JumpHeight");
                    float gravityControl = m_anim.GetFloat("GravityControl");
                    if (gravityControl > 0)
                        m_rb.useGravity = false;  //ジャンプ中の重力の影響を切る

                    // レイキャストをキャラクターのセンターから落とす
                    Ray ray = new Ray(m_tf.position + Vector3.up, -Vector3.up);
                    RaycastHit hitInfo = new RaycastHit();
                    // 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        if (hitInfo.distance > useCurvesHeight)
                        {
                            m_col.height = m_orgColHight - jumpHeight;          // 調整されたコライダーの高さ
                            float adjCenterY = m_orgVectColCenter.y + jumpHeight;
                            m_col.center = new Vector3(0, adjCenterY, 0); // 調整されたコライダーのセンター
                        }
                        else
                        {
                            // 閾値よりも低い時には初期値に戻す（念のため）					
                            resetCollider();
                        }
                    }
                }
                // Jump bool値をリセットする（ループしないようにする）				
                m_anim.SetBool("Jump", false);
            }
        }
        // IDLE中の処理
        // 現在のベースレイヤーがidleStateの時
        else if (m_currentBaseState.fullPathHash == idleState)
        {
            //カーブでコライダ調整をしている時は、念のためにリセットする
            if (useCurves)
            {
                resetCollider();
            }
            // スペースキーを入力したらRest状態になる
            if (Input.GetButtonDown("Jump"))
            {
                m_anim.SetBool("Rest", true);
            }
        }
        // REST中の処理
        // 現在のベースレイヤーがrestStateの時
        //else if (m_currentBaseState.fullPathHash == restState)
        //{
        //    //cameraObject.SendMessage("setCameraPositionFrontView");		// カメラを正面に切り替える
        //    // ステートが遷移中でない場合、Rest bool値をリセットする（ループしないようにする）
        //    if (!m_anim.IsInTransition(0))
        //    {
        //        m_anim.SetBool("Rest", false);
        //    }
        //}
    }
    // 以下、メイン処理.リジッドボディと絡めるので、FixedUpdate内で処理を行う.
    void FixedUpdate()
    {

        //キャラクターを移動させる
        m_rb.AddForce(m_moveVec * m_forwardSpeed, ForceMode.VelocityChange);











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

