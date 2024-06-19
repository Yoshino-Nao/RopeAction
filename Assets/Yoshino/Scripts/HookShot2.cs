using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using System.Linq;
public class HookShot2 : MonoBehaviour
{
    [SerializeField] private ObiSolver solver;
    //ロープオブジェクト
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

    //ロープをアタッチしているオブジェクトのコンポーネント
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
        //1フレーム待つ
        yield return null;

        //Pin Constraintsをクリア
        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();

        //ヒットした地点の座標をローカルの座標に変換
        Vector3 localHit = m_tf.InverseTransformPoint(TargetTf.position);
        //ロープ パスを手順に従って生成します (時間の経過とともに延長するため、短いセグメントのみ)。
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        float mass = 0.1f;

        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        //ロープのパーティクル表現を生成します (完了するまで待ちます)。
        yield return blueprint.Generate();

        //ブループリントを設定します(これにより、パーティクル/コンストレイントがソルバーに追加され、それらのシミュレーションが開始されます)。
        m_rope.ropeBlueprint = blueprint;

        //1フレーム待ちます
        yield return null;

        m_rope.GetComponent<MeshRenderer>().enabled = true;
        //m_obiStitcher.enabled = true;

        //ロープを伸ばすときに位置を上書きするので、質量をゼロに設定します。
        for (int i = 0; i < m_rope.activeParticleCount; ++i)
            solver.invMasses[m_rope.solverIndices[i]] = 0;



        float currentLength = 0;

        //最後のパーティクルがヒットした地点まで到達していない間、ロープを延長します。
        while (true)
        {
            //solverspaceでロープの起点を計算する
            Vector3 origin = solver.transform.InverseTransformPoint(m_rope.transform.position);

            //方向とフックポイントまでの距離を更新します。
            Vector3 direction = TargetTf.position - origin;
            float distance = direction.magnitude;
            direction.Normalize();

            //currentLengthを長くします:
            currentLength += hookShootSpeed * Time.deltaTime;

            //目的の長さに達したら、ループを中断します。
            if (currentLength >= distance)
            {
                cursor.ChangeLength(distance);
                break;
            }

            // ロープの長さを変更する（オーバーシュートを避けるために、ロープの起点とフックの間の距離に合わせてクランプします）
            cursor.ChangeLength(Mathf.Min(distance, currentLength));

            // すべてのパーティクルを順番に繰り返し、要素の長さを考慮して直線に配置します。
            float length = 0;
            for (int i = 0; i < m_rope.elements.Count; ++i)
            {
                solver.positions[m_rope.elements[i].particle1] = origin + direction * length;
                solver.positions[m_rope.elements[i].particle2] = origin + direction * (length + m_rope.elements[i].restLength);
                length += m_rope.elements[i].restLength;
            }

            //1フレーム待ちます
            yield return null;
        }

        //ロープが配置された時点でシミュレーションが引き継がれるように質量を復元します。
        for (int i = 0; i < m_rope.activeParticleCount; ++i)
            solver.invMasses[m_rope.solverIndices[i]] = 10; // 1/0.1 = 10

        ConnectToSelf(Target);
        m_player.StateChangeSetupOnRopeGrab(true);
        //m_player.GrabPointSetUp();
    }
    /// <summary>
    /// ロープとオブジェクトを繋ぐ
    /// </summary>
    /// <param name="collider1">スタート</param>
    /// <param name="collider2">エンド</param>
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
        //GrabPointを削除
    }
    public void HookShooting()
    {
        DebugPrint.Print(string.Format("AttachmentObj:{0}", m_attachmentTargetObj?.name));
        //右クリックで発射、解除
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
        //スペースキーで長さを縮小
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
        //シフトキーで長さを延長
        else if (Input.GetKey(KeyCode.LeftShift))
        {
            cursor.ChangeLength(Mathf.Clamp(m_rope.restLength + hookExtendRetractSpeed * Time.deltaTime, min, max));
        }
        DebugPrint.Print(string.Format("RopeLength{0}", m_rope.restLength));
        //マウスホイールで長さを変更
        cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Wheel * Time.deltaTime, min, max));
    }


    public GameObject FindRopeAttachmentObj()
    {
        Vector3 Origin = m_player.transform.position + Vector3.up * 0.1f;
        LayerMask layerMask = LayerMask.NameToLayer("Ropeattach");
        var hits = Physics.SphereCastAll(
            Origin,            //中心
            m_length,          //半径
            m_tf.forward,      //方向
            0f,                //長さ
            1 << layerMask     //レイヤーマスク
            ).Select(h => h.transform.gameObject).ToList();


        if (hits.Count > 0)
        {
            //オブジェクトが画面内かつ、間に障害物がないもののみリストアップする, ~(1 << LayerMask.NameToLayer("Player"))
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
                //距離を求める
                float Length = Vector2.Distance(new Vector2(0.5f, 0.5f), new Vector2(ViewPort.x, ViewPort.y));
                //attachmentObjとの間にコライダー付きのオブジェクトがあった場合は無視する
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


