using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using System.Linq;
public class HookShot2 : MonoBehaviour
{
    [SerializeField] private ObiSolver solver;
    //���[�v�I�u�W�F�N�g
    [SerializeField] private float m_length = 10f;

    [SerializeField] private ObiCollider character;
    [SerializeField] private Material material;
    [SerializeField] private ObiRopeSection section;
    [Range(0, 1)]
    [SerializeField] private float hookResolution = 0.5f;
    [SerializeField] private float hookExtendRetractSpeed = 2;
    [SerializeField] private float hookShootSpeed = 30;
    [SerializeField] private int particlePoolSize = 100;

    private Transform m_tf;
    private Rigidbody m_rb;
    public bool SetIsKinematic
    {
        set { m_rb.isKinematic = value; }
    }
    private UITest m_ui;
    private Camera m_mainCamera;

    private ObiRope m_rope;
    private ObiRopeBlueprint blueprint;
    private ObiRopeExtrudedRenderer ropeRenderer;
    private ObiRopeCursor cursor;
    private ObiConstraints<ObiPinConstraintsBatch> pinConstraints;
    [SerializeField] private ObiStitcher m_obiStitcher;

    private RaycastHit hookAttachment;

    [SerializeField] private GameObject m_attachmentTargetObj = null;

    //���[�v���A�^�b�`���Ă���I�u�W�F�N�g�̃R���|�[�l���g
    private Transform m_currentAttachTf;
    public Transform GetCurrnetAttachTf
    {
        get { return m_currentAttachTf; }
    }
    private Rigidbody m_currentAttachRb;
    public Rigidbody GetCurrnetAttachRb
    {
        get { return m_currentAttachRb; }
    }
    private ObiColliderBase m_currentAttachObiCol;
    public ObiColliderBase GetCurrnetAttachObiCol
    {
        get { return m_currentAttachObiCol; }
    }
    private Player m_player;
    [Button]
    void test()
    {
        LaunchHook();
    }


    public bool GetisLoaded
    {
        get
        {
            if (m_rope != null)
            {
                return m_rope.isLoaded;
            }
            return false;
        }
    }

    void OnEnable()
    {
        if (solver == null)
        {
            solver = GetComponentInParent<ObiSolver>();
        }


        m_tf = transform;
        m_rb = GetComponent<Rigidbody>();
        m_mainCamera = Camera.main;
        m_player = FindObjectOfType<Player>();
        m_ui = FindFirstObjectByType<UITest>();

        GameObject RopeObj = new GameObject("Rope");
        RopeObj.transform.parent = transform;
        RopeObj.transform.localPosition = Vector3.zero;

        // Create both the rope and the solver:	
        m_rope = RopeObj.AddComponent<ObiRope>();


        ropeRenderer = RopeObj.AddComponent<ObiRopeExtrudedRenderer>();
        ropeRenderer.section = section;
        ropeRenderer.uvScale = new Vector2(1, 4);
        ropeRenderer.normalizeV = false;
        ropeRenderer.uvAnchor = 1;
        ropeRenderer.thicknessScale = 0.2f;
        RopeObj.GetComponent<MeshRenderer>().material = material;

        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;
        blueprint.pooledParticles = particlePoolSize;

        // Tweak rope parameters:
        m_rope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        cursor = RopeObj.AddComponent<ObiRopeCursor>();
        cursor.cursorMu = 0;
        cursor.direction = true;

        //m_obiStitcher.Actor1 = m_rope;
        //m_obiStitcher.enabled = false;
        //m_grabObj.transform.position = m_tf.position;
        //m_grabPoint.SetParent(m_tf);
    }
    private void Start()
    {

    }

    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }


    public void LaunchHook()
    {
        if (m_attachmentTargetObj == null) return;

        Ray ray = new Ray(m_tf.position, m_attachmentTargetObj.transform.position - m_tf.position);
        //Vector3 vec = 
        m_currentAttachTf = m_attachmentTargetObj.transform;
        m_currentAttachRb = m_attachmentTargetObj.GetComponent<Rigidbody>();
        m_currentAttachObiCol = m_attachmentTargetObj.GetComponent<ObiColliderBase>();
        // Raycast to see what we hit:
        if (Physics.Raycast(ray, out hookAttachment, float.MaxValue, 1 << LayerMask.NameToLayer("Ropeattach")))
        {
            if (GetisLoaded)
            {
                DetachHook();
            }
            StartCoroutine(AttachHookForKinematic(m_currentAttachObiCol));

        }

    }

    private IEnumerator AttachHookForKinematic(ObiColliderBase Target)
    {
        Transform TargetTf = Target.transform;
        //1�t���[���҂�
        yield return null;

        //Pin Constraints���N���A
        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();

        //�q�b�g�����n�_�̍��W�����[�J���̍��W�ɕϊ�
        Vector3 localHit = m_tf.InverseTransformPoint(TargetTf.position);
        //���[�v �p�X���菇�ɏ]���Đ������܂� (���Ԃ̌o�߂ƂƂ��ɉ������邽�߁A�Z���Z�O�����g�̂�)�B
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        float mass = 0.1f;

        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        //���[�v�̃p�[�e�B�N���\���𐶐����܂� (��������܂ő҂��܂�)�B
        yield return blueprint.Generate();

        //�u���[�v�����g��ݒ肵�܂�(����ɂ��A�p�[�e�B�N��/�R���X�g���C���g���\���o�[�ɒǉ�����A�����̃V�~�����[�V�������J�n����܂�)�B
        m_rope.ropeBlueprint = blueprint;

        //1�t���[���҂��܂�
        yield return null;

        m_rope.GetComponent<MeshRenderer>().enabled = true;
        //m_obiStitcher.enabled = true;

        //���[�v��L�΂��Ƃ��Ɉʒu���㏑������̂ŁA���ʂ��[���ɐݒ肵�܂��B
        for (int i = 0; i < m_rope.activeParticleCount; ++i)
            solver.invMasses[m_rope.solverIndices[i]] = 0;



        float currentLength = 0;

        //�Ō�̃p�[�e�B�N�����q�b�g�����n�_�܂œ��B���Ă��Ȃ��ԁA���[�v���������܂��B
        while (true)
        {
            //solverspace�Ń��[�v�̋N�_���v�Z����
            Vector3 origin = solver.transform.InverseTransformPoint(m_rope.transform.position);

            //�����ƃt�b�N�|�C���g�܂ł̋������X�V���܂��B
            Vector3 direction = TargetTf.position - origin;
            float distance = direction.magnitude;
            direction.Normalize();

            //currentLength�𒷂����܂�:
            currentLength += hookShootSpeed * Time.deltaTime;

            //�ړI�̒����ɒB������A���[�v�𒆒f���܂��B
            if (currentLength >= distance)
            {
                cursor.ChangeLength(distance);
                break;
            }

            // ���[�v�̒�����ύX����i�I�[�o�[�V���[�g������邽�߂ɁA���[�v�̋N�_�ƃt�b�N�̊Ԃ̋����ɍ��킹�ăN�����v���܂��j
            cursor.ChangeLength(Mathf.Min(distance, currentLength));

            // ���ׂẴp�[�e�B�N�������ԂɌJ��Ԃ��A�v�f�̒������l�����Ē����ɔz�u���܂��B
            float length = 0;
            for (int i = 0; i < m_rope.elements.Count; ++i)
            {
                solver.positions[m_rope.elements[i].particle1] = origin + direction * length;
                solver.positions[m_rope.elements[i].particle2] = origin + direction * (length + m_rope.elements[i].restLength);
                length += m_rope.elements[i].restLength;
            }

            //1�t���[���҂��܂�
            yield return null;
        }

        //���[�v���z�u���ꂽ���_�ŃV�~�����[�V�����������p�����悤�Ɏ��ʂ𕜌����܂��B
        for (int i = 0; i < m_rope.activeParticleCount; ++i)
            solver.invMasses[m_rope.solverIndices[i]] = 10; // 1/0.1 = 10

        ConnectToSelf(Target);
        m_player.StateChangeSetupOnRopeGrab(true);
        //m_player.GrabPointSetUp();
    }
    /// <summary>
    /// ���[�v�ƃI�u�W�F�N�g���q��
    /// </summary>
    /// <param name="collider1">�X�^�[�g</param>
    /// <param name="collider2">�G���h</param>
    public void ConnectToOtherObj(ObiColliderBase collider1, ObiColliderBase collider2)
    {
        if (m_currentAttachObiCol == null) return;

        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(
            m_rope.elements[0].particle1,
            collider1,
            Vector3.zero,
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.AddConstraint(
            m_rope.elements[m_rope.elements.Count - 1].particle2,
            collider2,
            collider2.transform.InverseTransformPoint(collider2.transform.position),
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        m_rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }
    public void ConnectToPlayer(ObiColliderBase Playerobicol, Vector3 GrabPos)
    {
        //pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        //pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(
            m_rope.elements[0].particle1,
            Playerobicol,
            GrabPos,
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.AddConstraint(
            m_rope.elements[m_rope.elements.Count - 1].particle2,
            m_currentAttachObiCol,
            m_currentAttachObiCol.transform.InverseTransformPoint(m_currentAttachObiCol.transform.position),
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );

        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        m_rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }
    public void ConnectToSelf(ObiColliderBase Target)
    {
        //pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        //pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(
            m_rope.elements[0].particle1,
            character,
            Vector3.zero,
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.AddConstraint(
            m_rope.elements[m_rope.elements.Count - 1].particle2,
            Target,
            Target.transform.InverseTransformPoint(Target.transform.position),
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );

        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        m_rope.SetConstraintsDirty(Oni.ConstraintType.Pin);

    }

    public void Setup()
    {
        m_tf.parent = solver.transform;
        //m_player.transform.parent = m_tf;
        //m_player.transform.position = Vector3.zero;
    }

    public void DetachHook()
    {
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        m_rope.ropeBlueprint = null;
        m_rope.GetComponent<MeshRenderer>().enabled = false;
        //m_obiStitcher.enabled = false;
        //GrabPoint���폜
    }
    public void HookShooting()
    {
        DebugPrint.Print(string.Format("AttachmentObj:{0}", m_attachmentTargetObj?.name));
        //�E�N���b�N�Ŕ��ˁA����
        if (Input.GetMouseButtonDown(1))
        {
            if (!m_rope.isLoaded)
                LaunchHook();
            else
                DetachHook();
        }
    }
    public void RopeChangeLength()
    {
        if (!m_rope.isLoaded) return;
        float Wheel = Input.GetAxis("Mouse ScrollWheel");
        float max = 20f;
        float min = 0.5f;
        float Dist = Vector3.Distance(m_tf.position, m_currentAttachTf.position);
        float FinalMin = Mathf.Max(Dist, min);
        if (MPFT_NTD_MMControlSystem.ms_instance != null)
        {
            if (MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.Down)
            {
                cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Time.deltaTime, min, max));
            }
        }
        //�X�y�[�X�L�[�Œ������k��
        else if (Input.GetKey(KeyCode.Space))
        {
            cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Time.deltaTime, min, max));
        }
        if (MPFT_NTD_MMControlSystem.ms_instance != null)
        {
            if (MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.Up)
            {
                cursor.ChangeLength(Mathf.Clamp(m_rope.restLength + hookExtendRetractSpeed * Time.deltaTime, min, max));
            }
        }
        //�V�t�g�L�[�Œ���������
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            cursor.ChangeLength(Mathf.Clamp(m_rope.restLength + hookExtendRetractSpeed * Time.deltaTime, min, max));
        }
        DebugPrint.Print(string.Format("RopeLength{0}", m_rope.restLength));
        //�}�E�X�z�C�[���Œ�����ύX
        cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Wheel * Time.deltaTime, min, max));
    }


    public GameObject FindRopeAttachmentObj()
    {
        Vector3 Origin = m_player.transform.position + Vector3.up * 0.1f;
        LayerMask layerMask = LayerMask.NameToLayer("Ropeattach");
        var hits = Physics.SphereCastAll(
            Origin,            //���S
            m_length,          //���a
            m_tf.forward,      //����
            0f,                //����
            1 << layerMask     //���C���[�}�X�N
            ).Select(h => h.transform.gameObject).ToList();


        if (hits.Count > 0)
        {
            //�I�u�W�F�N�g����ʓ����A�Ԃɏ�Q�����Ȃ����̂̂݃��X�g�A�b�v����, ~(1 << LayerMask.NameToLayer("Player"))
            hits = hits.Where(_ =>
            {
                Vector3 vec = m_mainCamera.WorldToViewportPoint(_.transform.position);
                return
                vec.x > 0 && vec.x < 1 &&
                vec.y > 0 && vec.y < 1;
            }).ToList();

            GameObject obj = null;
            float MinLength = float.MaxValue;
            LayerMask Mask = 1 << LayerMask.NameToLayer("Ground");
            foreach (var hit in hits)
            {
                Vector3 ViewPort = m_mainCamera.WorldToViewportPoint(hit.transform.position);
                //���������߂�
                float Length = Vector2.Distance(new Vector2(0.5f, 0.5f), new Vector2(ViewPort.x, ViewPort.y));
                //attachmentObj�Ƃ̊ԂɃR���C�_�[�t���̃I�u�W�F�N�g���������ꍇ�͖�������
                if (Physics.Raycast(Origin, hit.transform.position - Origin, out RaycastHit hitInfo, m_length, Mask))
                {
                    //DebugPrint.Print(string.Format("1{0}", hit));
                    //DebugPrint.Print(string.Format("2{0}", hitInfo.collider.gameObject));
                    if (hit != hitInfo.collider.gameObject)
                    {
                        continue;
                    }
                }
                if (Length < MinLength)
                {
                    MinLength = Length;
                    obj = hit;
                }

            }
            return obj;
        }
        else
        {
            return null;
        }
    }
    private void Update()
    {
        m_attachmentTargetObj = FindRopeAttachmentObj();
        if (m_currentAttachObiCol != null)
        {
            DebugPrint.Print(string.Format("{0}", m_currentAttachObiCol.name));
        }
        //  m_grabMesh.enabled = false;
        if (m_ui != null)
        {
            m_ui.SetAttachmentTarget = m_attachmentTargetObj?.transform;
        }
    }
}


