using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IceMilkTea.StateMachine;
using VInspector;
using Obi;
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
    [SerializeField] private float m_animSpeed = 1.5f;              // �A�j���[�V�����Đ����x�ݒ�
    [SerializeField] private float m_lookSmoother = 3.0f;           // a smoothing setting for camera motion
    [SerializeField] private bool useCurves = true;               // Mecanim�ŃJ�[�u�������g�����ݒ肷��
                                                                  // ���̃X�C�b�`�������Ă��Ȃ��ƃJ�[�u�͎g���Ȃ�
    [SerializeField] private float useCurvesHeight = 0.5f;        // �J�[�u�␳�̗L�������i�n�ʂ����蔲���₷�����ɂ͑傫������j

    // �ȉ��L�����N�^�[�R���g���[���p�p�����^
    // �O�i���x
    [SerializeField] private float m_forwardSpeed = 7.0f;
    // ��ޑ��x
    [SerializeField] private float m_backwardSpeed = 2.0f;
    //�ō����x
    [SerializeField] private float m_maxSpeed = 20f;
    // ���񑬓x
    [SerializeField] private float m_rotateSpeed = 1500.0f;
    // �W�����v�З�
    [SerializeField] private float m_jumpPower = 280f;

    [SerializeField] Vector3 m_playerForwardOn2d = Vector3.right;

    [SerializeField] private float m_grabTotalTime = 1f;
    //�v���C���[�������Ă���R���|�[�l���g
    private Transform m_tf => transform;
    private CapsuleCollider m_capsuleCol;
    private Rigidbody m_rb;
    private PhysicMaterial m_physicMaterial;

    //�L�����N�^�[���f���������Ă���R���|�[�l���g
    private FullBodyBipedIK m_fullBodyBipedIK;

    //�A�j���[�V�����֌W
    private Animator m_anim;
    private AnimatorStateInfo m_currentBaseState;
    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int MoveState = Animator.StringToHash("Base Layer.Blend Tree");
    static int jumpState = Animator.StringToHash("Base Layer.Jump");
    static int fallState = Animator.StringToHash("Base Layer.Fall");
    static int restState = Animator.StringToHash("Base Layer.Rest");

    //�ړ��Ɏg�p����
    private Vector3 m_moveDir;
    private Vector3 m_moveVec;
    private float m_moveSpeed;
    private Vector3 m_inputMoveDir;
    private Vector3 m_normal;
    private Vector3 m_oldPos;

    //�ڒn����Ŏg�p����
    private bool m_isGround;
    public bool GetIsGround { get { return m_isGround; } }
    private bool m_oldIsGround;
    private float m_orgColHight;
    private Vector3 m_orgVectColCenter;
    private RaycastHit m_groundHit;

    //
    //�J������Transform
    private Transform m_CameraTf;

    //���[�v���ˊ֌W
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
    [SerializeField] private Transform m_grabPos;
    [Button]
    void Test()
    {
        if (m_grabbable != null)
        {
            m_grabbable.SetArmIKTarget(ref m_fullBodyBipedIK);
            SetIKWeight(1);
            StartCoroutine(RopeGrab());
        }
    }

    #region �ړ��֌W

    private void SetMoveDir()
    {
        float h;
        float v;
        m_inputMoveDir = Vector3.zero;
        if (MPFT_NTD_MMControlSystem.ms_instance != null)
        {
            h = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_X;
            v = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.L_Analog_Y;
        }
        else
        {
            h = Input.GetAxis("Horizontal");              // ���̓f�o�C�X�̐�������h�Œ�`
            v = Input.GetAxis("Vertical");                // ���̓f�o�C�X�̐�������v�Œ�`

            m_inputMoveDir = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
        //�J������2D��3D������
        if (CameraChanger.ms_instance.m_is3DCamera)
        {
            //3D�̏ꍇ�̓J�����Ɉˑ�����
            Vector3 Forward = Vector3.Scale(m_CameraTf.forward, new Vector3(1, 0, 1)).normalized;
            m_moveDir = (Forward * v + m_CameraTf.right * h);
        }
        else
        {
            //2D�̏ꍇ�͍��E�̓��݂͂̂��ړ��Ɏg�p����
            m_moveDir = (m_playerForwardOn2d * h);

        }

        DebugPrint.Print(string.Format("MoveVec{0}", m_moveDir));
    }
    private void Move()
    {
        //�A�j���[�V����
        Vector3 Vec = m_tf.InverseTransformDirection(m_moveDir);
        //DebugPrint.Print(string.Format("AnimVec{0}", Vec));
        // Animator���Őݒ肵�Ă���"Speed"�p�����^��n��
        if (m_isGround)
        {
            m_anim.SetFloat("SpeedX", Vec.x);
            m_anim.SetFloat("SpeedY", Vec.z);
        }

        //�ړ����͂����邩
        if (m_inputMoveDir.magnitude > 0)
        {
            //����Έړ��p�x�N�g����ݒ�
            m_moveVec = m_moveDir;
            m_normal = m_groundHit.normal;
        }
        else
        {
            //�Ȃ���Έړ��p�x�N�g����0�ɂ���
            m_moveVec = Vector3.zero;
        }
        //if (!m_isGrabbing)
        {
            //�ړ������ɉ�]���鏈��
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
    }
    private void RopeJump()
    {
        if (m_hookShot.GetCurrnetAttachRb.isKinematic)
        {
            m_rb.AddForce(m_tf.forward * m_jumpPower, ForceMode.Impulse);
            Release();
        }
    }
    #endregion

    #region ���[�v�֌W
    private void Launch()
    {
        m_hookShot.LaunchHook();

        m_grabbable.SetArmIKTarget(ref m_fullBodyBipedIK);
        SetIKWeight(1);

        StartCoroutine(RopeGrab());
    }
    private void Detach()
    {
        m_hookShot.DetachHook();
    }
    public void Release()
    {
        //    m_hookShot.SetGrabMesh(false);
        //    m_hookShot.ConnectCurrentObjToOtherObj(m_grabPoint.GetObiCol);
        //    //IK���������������Z���J�n
        //    SetIKWeight(0);
        //    m_grabPoint.SetParent(null);
        //    m_grabPoint.EnablePhysics();


        //    m_isGrabbing = false;
        //    Debug.Log("���[�v�𗣂��܂���");
    }
    private void Conntect()
    {
        Release();
        //m_hookShot.ConnectToOtherObj(m_hookShot.GetAttachmentTargetObiCol);
    }
    public void GrabPointSetUp()
    {
        //m_grabPoint.SetUp();
        //SetIKWeight(1);
        //m_grabPoint.SetParent(m_tf);
        //Debug.Log("���[�v��݂͂܂���");
        //m_isGrabbing = true;
    }
    private void RopeGrabbingOnGround()
    {
        m_hookShot.RopeChangeLength();

        //���[�v���������Ă���I�u�W�F�N�g�̕����Ɍ���������
        Vector3 ToGrabPointDir = (m_hookShot.GetCurrnetAttachTf.position - m_tf.position).normalized;
        // ToAttachPointDir = (m_hookShot.GetCurrnetAttachTf.position - m_tf.position + new Vector3(0, 0, m_hookShot.transform.localPosition.z)).normalized;
        Debug.DrawRay(m_tf.position, ToGrabPointDir * 100);

        ToGrabPointDir = Vector3.ProjectOnPlane(ToGrabPointDir, Vector3.up);
        m_tf.rotation = Quaternion.LookRotation(ToGrabPointDir, Vector3.up);


    }
    private void RopeGrabbingOnAir()
    {
        m_hookShot.RopeChangeLength();

        //���[�v���������Ă���I�u�W�F�N�g�̕����Ɍ���������
        //Vector3 ToGrabPointDir = (m_grabPoint.transform.position - m_tf.position).normalized;
        Vector3 ToAttachPointDir = (m_hookShot.GetCurrnetAttachTf.position - m_tf.position + new Vector3(0, 0, m_hookShot.transform.localPosition.z)).normalized;
        //Debug.DrawRay(m_tf.position, ToGrabPointDir * 100);
        if (m_hookShot.GetCurrnetAttachRb.isKinematic)
        {
            if (m_moveDir.magnitude > 0)
            {
                m_moveDir = Vector3.ProjectOnPlane(m_moveDir, ToAttachPointDir);
                m_tf.rotation = Quaternion.RotateTowards(m_tf.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(m_moveDir, ToAttachPointDir)), m_rotateSpeed * Time.fixedDeltaTime);
            }
            else
            {
                m_tf.rotation = Quaternion.RotateTowards(m_tf.rotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(m_tf.forward, Vector3.up)), m_rotateSpeed * Time.fixedDeltaTime);
            }
        }
    }
    #endregion
    private bool FootCollider()
    {
        return (Physics.SphereCast(m_tf.position + m_capsuleCol.center, m_capsuleCol.radius, -m_tf.up, out m_groundHit, m_capsuleCol.height / 1.5f, 1 << LayerMask.NameToLayer("Ground")));
    }
    private void LandingAndJumpSetUp(bool IsGround)
    {
        if (IsGround)
        {
            //�ڒn���͖��C�͂��P��
            m_physicMaterial.dynamicFriction = 1;
            m_physicMaterial.staticFriction = 1;
            //�d�͂𖳌���
            m_rb.useGravity = false;
        }
        else
        {

            //�󒆂ł͖��C�͂�0�ɂ��āA�ǂɓ������Ă���Ƃ��󒆂ŗ��܂邱�Ƃ�h��
            m_physicMaterial.dynamicFriction = 0;
            m_physicMaterial.staticFriction = 0;
            //�d�͂�L����
            m_rb.useGravity = true;
        }
    }
    //�͂�
    public bool LerpGrabbable()
    {
        m_lerpTForGrabbable += Time.deltaTime / m_grabTotalTime;

        m_grabbable.transform.position = Vector3.Lerp(
            m_grabbable.transform.position,
            m_grabPos.transform.position,
            m_lerpTForGrabbable
            );

        //m_ikTarget.Move(m_grabPoint.transform.position);

        return m_lerpTForGrabbable >= 1;
    }
    private void ConnectToPlayer()
    {
        m_hookShot.ConnectToPlayer(m_obiCollider, m_grabPos.localPosition);
    }
    private void EnablePhysics()
    {
        m_hookShot.ConnectToSelf(m_hookShot.GetCurrnetAttachObiCol);
    }
    public void SetIKWeight(float weight)
    {
        m_fullBodyBipedIK.solver.leftHandEffector.positionWeight = weight;
        m_fullBodyBipedIK.solver.leftHandEffector.rotationWeight = weight;
        m_fullBodyBipedIK.solver.rightHandEffector.positionWeight = weight;
        m_fullBodyBipedIK.solver.rightHandEffector.rotationWeight = weight;
    }
    public IEnumerator RopeGrab()
    {
        m_grabbable.SetParent(null);

        m_lerpTForGrabbable = 0;
        yield return new WaitUntil(() => LerpGrabbable());
        m_lerpTForGrabbable = 0;
        m_isGrabbing = true;

        m_grabbable.SetParent(m_tf);
        Debug.Log("���[�v��݂͂܂���");
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

        m_hookShotTf = m_hookShot.transform;
        m_grabbable.SetUp();
        //�^���I�Ȑe�q�֌W������
        m_grabbable.SetParent(m_tf);



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

        m_anim.SetBool("Ground", m_isGround);

        //m_hookShot.HookShooting();
        DebugPrint.Print(string.Format("CurrentStateName:{0}", stateMachine.CurrentStateName));
        stateMachine.Update();
    }
    private void FixedUpdate()
    {
        //if (m_isGround != m_oldIsGround)
        //{
        //    LandingAndJumpSetUp(m_isGround);
        //}
        m_rb.AddForce(Vector3.ProjectOnPlane(m_moveVec, m_normal) * m_forwardSpeed, ForceMode.Acceleration);
    }
    private class GroundState : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();
            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(true);

            Context.SetIKWeight(0);
            Context.m_anim.SetBool("Jump", false);
        }
        protected internal override void Update()
        {
            base.Update();
            //��ԑJ��
            if (!Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.Air);
            }

            if (Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnGround);
            }

            DebugPrint.Print(string.Format("Test"));

            //�ړ�����
            Context.Move();

            //��̈ʒu�̐���
            //if (Context.m_grabPoint != null)
            //{
            //    float dist = Vector3.Distance(Context.m_tf.position, Context.m_grabPoint.transform.position);
            //    float length = 2f;
            //    if (Input.GetKeyDown(KeyCode.V) && dist <= length)
            //    {
            //        Context.StartCoroutine(Context.Grab());
            //    }
            //}
            //���[�v����
            if (Input.GetMouseButtonDown(1))
            {
                Context.Launch();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.MM_TR)
            {
                Context.Launch();
            }



            //�W�����v
            if (Input.GetButtonDown("Jump"))
            {
                Context.Jump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
            {
                Context.Jump();
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
        }
        protected internal override void Update()
        {
            base.Update();
            if (!Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnAir);
            }
            if (!Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.Ground);
            }
            Context.Move();
            Context.RopeGrabbingOnGround();

            //�W�����v
            if (Input.GetButtonDown("Jump"))
            {
                Context.Jump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
            {
                Context.Jump();
            }
            //���[�v�𗣂�
            if (Input.GetKeyDown(KeyCode.V))
            {
                Context.Release();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
            {
                Context.Jump();
            }
            //���[�v������
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
        }
    }
    private class GrabbingRopeOnAir : MyState
    {
        protected internal override void Enter()
        {
            base.Enter();
            Debug.Log(stateMachine.CurrentStateName);
            Context.LandingAndJumpSetUp(false);
            Context.ConnectToPlayer();
        }
        protected internal override void Update()
        {
            base.Update();
            if (Context.m_isGround)
            {
                stateMachine.SendEvent(StateEvent.GrabbingRopeOnGround);
            }
            if (!Context.m_isGrabbing)
            {
                stateMachine.SendEvent(StateEvent.Air);
            }
            //Context.Move();
            Context.RopeGrabbingOnAir();

            if (Input.GetButtonDown("Jump"))
            {
                Context.RopeJump();
            }
            if (MPFT_NTD_MMControlSystem.ms_instance != null && MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.B)
            {
                Context.Jump();
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
            Context.EnablePhysics();
        }
    }
}
