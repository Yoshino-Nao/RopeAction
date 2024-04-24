using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class HookShot : MonoBehaviour
{
    public ObiSolver solver;
    public ObiCollider character;
    [SerializeField] private Transform RightHand;
    [SerializeField] private Transform LeftHand;
    public Material material;
    public ObiRopeSection section;

    [Range(0, 1)]
    public float hookResolution = 0.5f;
    public float hookExtendRetractSpeed = 2;
    public float hookShootSpeed = 30;
    public int particlePoolSize = 100;

    private ObiRope rope;
    private ObiRopeBlueprint blueprint;

    private ObiRopeCursor cursor;
    ObiConstraints<ObiPinConstraintsBatch> pinConstraints;
    private ObiPinConstraintsBatch batch1;

    private RaycastHit hookAttachment;
    private ObiColliderBase hookAttachedColl;
    public bool GetisLoaded
    {
        get
        {
            if (rope != null)
            {
                return rope.isLoaded;
            }
            return false;
        }
    }

    void Awake()
    {

        // Create both the rope and the solver:	
        //rope = gameObject.AddComponent<ObiRope>();
        rope = gameObject.GetComponent<ObiRope>();
        //ropeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
        //ropeRenderer.section = section;
        //ropeRenderer.uvScale = new Vector2(1, 4);
        //ropeRenderer.normalizeV = false;
        //ropeRenderer.uvAnchor = 1;
        //rope.GetComponent<MeshRenderer>().material = material;

        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;
        blueprint.pooledParticles = particlePoolSize;

        // Tweak rope parameters:
        //rope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        //cursor = rope.gameObject.AddComponent<ObiRopeCursor>();
        cursor = rope.gameObject.GetComponent<ObiRopeCursor>();
        //cursor.cursorMu = 0;
        //cursor.direction = true;
    }

    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }

    /**
	 * �V�[���ɑ΂��ă��C�L���X�g���āA�t�b�N�������Ɏ��t�����邩�ǂ������m�F���܂��B
	 */
    private void LaunchHook()
    {

        //���̃I�u�W�F�N�g�Ɠ��� XY ���ʓ��̃V�[�����̃}�E�X�̈ʒu���擾���܂��B
        Vector3 mouse = Input.mousePosition;
        mouse.z = transform.position.z - Camera.main.transform.position.z;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        //�L�����N�^�[����}�E�X���W�ւ�Ray���擾���܂��B
        Ray ray = new Ray(transform.position, mouseInScene - transform.position);

        // Raycast to see what we hit:
        if (Physics.Raycast(ray, out hookAttachment))
        {
            // We actually hit something, so attach the hook!
            StartCoroutine(AttachHook());
        }

    }

    private IEnumerator AttachHook()
    {
        yield return null;

        //Pin Constraints���N���A
        pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        //�q�b�g�����n�_�̍��W�����[�J���̍��W�ɕϊ�
        Vector3 localHit = rope.transform.InverseTransformPoint(hookAttachment.point);
        //���[�v �p�X���菇�ɏ]���Đ������܂� (���Ԃ̌o�߂ƂƂ��ɉ������邽�߁A�Z���Z�O�����g�̂�)�B
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        //���[�v�̃p�[�e�B�N���\���𐶐����܂� (��������܂ő҂��܂�)�B
        yield return blueprint.Generate();

        //�u���[�v�����g��ݒ肵�܂�(����ɂ��A�p�[�e�B�N��/�R���X�g���C���g���\���o�[�ɒǉ�����A�����̃V�~�����[�V�������J�n����܂�)�B
        rope.ropeBlueprint = blueprint;

        //1�t���[���҂��܂�
        yield return null;

        rope.GetComponent<MeshRenderer>().enabled = true;

        //���[�v��L�΂��Ƃ��Ɉʒu���㏑������̂ŁA���ʂ��[���ɐݒ肵�܂��B
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 0;
        float currentLength = 0;

        //�Ō�̃p�[�e�B�N�����q�b�g�����n�_�܂œ��B���Ă��Ȃ��ԁA���[�v���������܂��B
        while (true)
        {
            //solverspace�Ń��[�v�̋N�_���v�Z����
            Vector3 origin = solver.transform.InverseTransformPoint(rope.transform.position);

            //�����ƃt�b�N�|�C���g�܂ł̋������X�V���܂��B
            Vector3 direction = hookAttachment.point - origin;
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
            for (int i = 0; i < rope.elements.Count; ++i)
            {
                solver.positions[rope.elements[i].particle1] = origin + direction * length;
                solver.positions[rope.elements[i].particle2] = origin + direction * (length + rope.elements[i].restLength);
                length += rope.elements[i].restLength;
            }

            //1�t���[���҂��܂�
            yield return null;
        }

        //���[�v���z�u���ꂽ���_�ŃV�~�����[�V�����������p�����悤�Ɏ��ʂ𕜌����܂��B
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 10; // 1/0.1 = 10

        //���[�v�̗��[���s���ŌŒ肵�܂� (����ɂ��A�L�����N�^�[�ƃ��[�v�̊Ԃ̑o�����̃C���^���N�V�������\�ɂȂ�܂�)�B
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(rope.elements[0].particle1, character, transform.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(rope.elements[rope.elements.Count - 1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        hookAttachedColl = batch.pinBodies[0].owner;
        batch1 = batch;
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }

    private void DetachHook()
    {
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        rope.ropeBlueprint = null;
        rope.GetComponent<MeshRenderer>().enabled = false;
    }


    void Update()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (!rope.isLoaded)
                LaunchHook();
            else
                DetachHook();
        }

        if (rope.isLoaded)
        {
            //foreach (var a in batch1.offsets)
            //{
            //    Debug.Log(transform.InverseTransformPoint(a));
            //}
            
            if (Input.GetKey(KeyCode.Space))
            {
                cursor.ChangeLength(rope.restLength - hookExtendRetractSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.LeftShift))
            {
                cursor.ChangeLength(rope.restLength + hookExtendRetractSpeed * Time.deltaTime);
            }
        }
    }

}
