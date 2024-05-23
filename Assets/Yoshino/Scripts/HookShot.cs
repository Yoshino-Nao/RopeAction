using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using System.Linq;

public class HookShot : MonoBehaviour
{
    [SerializeField] private float m_langth = 10f;
    public ObiSolver solver;
    public ObiCollider character;
    public Material material;
    public ObiRopeSection section;

    [Range(0, 1)]
    public float hookResolution = 0.5f;
    public float hookExtendRetractSpeed = 2;
    public float hookShootSpeed = 30;
    public int particlePoolSize = 100;

    private Transform m_tf;
    private SphereCollider m_col;
    private Rigidbody m_rb;
    private CameraChanger m_cameraChanger = null;
    [SerializeField] private UITest m_ui;
    private ObiRope Obirope;
    [SerializeField] private MeshRenderer m_grabMesh;
    public bool SetGrabMeshRendererEnabled
    {
        set { m_grabMesh.enabled = value; }
    }
    private ObiRopeBlueprint blueprint;
    private ObiRopeCursor cursor;
    ObiConstraints<ObiPinConstraintsBatch> pinConstraints;


    private RaycastHit hookAttachment;
    //ロープをアタッチしているオブジェクト
    private GameObject AttachmentObj = null;
    public GameObject GetAttachmentObj
    {
        get { return AttachmentObj; }
    }
    private GameObject m_currentAttachObj;
    public GameObject GetCurrnetAttachObj
    {
        get { return m_currentAttachObj; }
    }
    private MoveTest m_player;

    private VirtualChildBehaviour m_childBehaviour;
    private bool m_toggle = false;

    [SerializeField] private GrabPoint m_grabPoint;
    private GameObject m_grabObj;
    private bool m_isGrabbing = false;

    [SerializeField] private ObiColliderBase Test;
    [Button]
    void ConnectTest()
    {
        ConnectToObj(Test);
    }

    //ロープをつけられるオブジェクトまでの一番短い距離
    private float MinLength = 999;

    public bool GetisLoaded
    {
        get
        {
            if (Obirope != null)
            {
                return Obirope.isLoaded;
            }
            return false;
        }
    }

    void Awake()
    {
        if (solver == null)
        {
            solver = GetComponentInParent<ObiSolver>();
        }


        m_tf = transform;
        m_col = GetComponent<SphereCollider>();
        m_rb = GetComponent<Rigidbody>();
        m_cameraChanger = GetComponentInParent<CameraChanger>();
        m_player = FindObjectOfType<MoveTest>();
        m_childBehaviour = GetComponent<VirtualChildBehaviour>();
        m_grabMesh.enabled = false;
        // Create both the rope and the solver:	
        //rope = gameObject.AddComponent<ObiRope>();
        Obirope = gameObject.GetComponent<ObiRope>();
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
        cursor = Obirope.gameObject.GetComponent<ObiRopeCursor>();
        //cursor.cursorMu = 0;
        //cursor.direction = true;


    }

    private void OnDestroy()
    {
        DestroyImmediate(blueprint);
    }

    /**
	 * シーンに対してレイキャストして、フックを何かに取り付けられるかどうかを確認します。
	 */
    public void LaunchHook()
    {
        if (AttachmentObj == null) return;
        //このオブジェクトと同じ XY 平面内のシーン内のマウスの位置を取得します。
        //Vector3 mouse = Input.mousePosition;
        //mouse.z = transform.position.z - Camera.main.transform.position.z;
        //Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);


        Ray ray = new Ray(m_tf.position, AttachmentObj.transform.position - transform.position);
        //Vector3 vec = 
        m_currentAttachObj = AttachmentObj;
        // Raycast to see what we hit:
        if (Physics.Raycast(ray, out hookAttachment))
        {
            // We actually hit something, so attach the hook!
            StartCoroutine(AttachHook());
        }

    }

    private IEnumerator AttachHook()
    {
        //1フレーム待つ
        yield return null;

        //Pin Constraintsをクリア
        pinConstraints = Obirope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        //ヒットした地点の座標をローカルの座標に変換
        Vector3 localHit = Obirope.transform.InverseTransformPoint(hookAttachment.point);
        //ロープ パスを手順に従って生成します (時間の経過とともに延長するため、短いセグメントのみ)。
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        //int Count = 10;
        //for (int i = 0; i < Count; i++)
        //{
        //    blueprint.path.AddControlPoint(localHit.normalized * 0.5f * i, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook " + i.ToString());
        //}
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        //ロープのパーティクル表現を生成します (完了するまで待ちます)。
        yield return blueprint.Generate();

        //ブループリントを設定します(これにより、パーティクル/コンストレイントがソルバーに追加され、それらのシミュレーションが開始されます)。
        Obirope.ropeBlueprint = blueprint;

        //1フレーム待ちます
        yield return null;

        Obirope.GetComponent<MeshRenderer>().enabled = true;



        //ロープを伸ばすときに位置を上書きするので、質量をゼロに設定します。
        for (int i = 0; i < Obirope.activeParticleCount; ++i)
            solver.invMasses[Obirope.solverIndices[i]] = 0;
        float currentLength = 0;

        //最後のパーティクルがヒットした地点まで到達していない間、ロープを延長します。
        while (true)
        {
            //solverspaceでロープの起点を計算する
            Vector3 origin = solver.transform.InverseTransformPoint(Obirope.transform.position);

            //方向とフックポイントまでの距離を更新します。
            Vector3 direction = hookAttachment.point - origin;
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
            for (int i = 0; i < Obirope.elements.Count; ++i)
            {
                solver.positions[Obirope.elements[i].particle1] = origin + direction * length;
                solver.positions[Obirope.elements[i].particle2] = origin + direction * (length + Obirope.elements[i].restLength);
                length += Obirope.elements[i].restLength;
            }

            //1フレーム待ちます
            yield return null;
        }

        //ロープが配置された時点でシミュレーションが引き継がれるように質量を復元します。
        for (int i = 0; i < Obirope.activeParticleCount; ++i)
            solver.invMasses[Obirope.solverIndices[i]] = 10; // 1/0.1 = 10
        PlayerGrabs();
        //ロープの両端をピンで固定します (これにより、キャラクターとロープの間の双方向のインタラクションが可能になります)。
        //var batch = new ObiPinConstraintsBatch();
        //batch.AddConstraint(Obirope.elements[0].particle1, character, m_tf.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        //batch.AddConstraint(Obirope.elements[Obirope.elements.Count - 1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
        //                                                  hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);

        //ObiColliderBase obiCollider = m_grabObj.GetComponent<ObiColliderBase>();
        //batch.AddConstraint(Obirope.elements[0].particle1, obiCollider, Vector3.zero, Quaternion.identity, 0, 0, float.PositiveInfinity);
        //batch.AddConstraint(Obirope.elements[Obirope.elements.Count - 1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
        //                                                  hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);

        //batch.activeConstraintCount = 2;
        //pinConstraints.AddBatch(batch);
        //Obirope.SetConstraintsDirty(Oni.ConstraintType.Pin);
        //GrabPointを生成しロープをGrabPointと繋ぐ
        m_grabObj = Instantiate(m_grabPoint.gameObject, m_tf.position, Quaternion.identity, solver.transform);
        m_player.SetGrabPoint = m_grabObj.GetComponent<GrabPoint>();
        m_player.GrabPointSetUp();
        //ConnectToObj(m_player.GetComponent<ObiColliderBase>());
        //StartCoroutine(m_player.Grab());
    }
    public void ClearPinConstraints()
    {
        //pinConstraints = Obirope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        //pinConstraints.Clear();
    }
    /// <summary>
    /// ロープとオブジェクトを繋ぐ
    /// </summary>
    public void ConnectToObj(ObiColliderBase collider)
    {
        pinConstraints = Obirope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(Obirope.elements[0].particle1, collider, Vector3.zero, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(Obirope.elements[Obirope.elements.Count - 1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        Obirope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }
    public void PlayerGrabs()
    {
        m_grabMesh.enabled = true;
        pinConstraints = Obirope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(Obirope.elements[0].particle1, character, m_tf.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.AddConstraint(Obirope.elements[Obirope.elements.Count - 1].particle2, hookAttachment.collider.GetComponent<ObiColliderBase>(),
                                                          hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point), Quaternion.identity, 0, 0, float.PositiveInfinity);
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        Obirope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }
    /// <summary>
    /// Hookの当たり判定をなくす
    /// </summary>
    public void ReleaseRope()
    {

    }

    private void DetachHook()
    {
        m_grabMesh.enabled = false;
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        Obirope.ropeBlueprint = null;
        Obirope.GetComponent<MeshRenderer>().enabled = false;
        //プレイヤーのIKをリセット
        m_player.SetIKWeight(0);
        m_player.m_isGrabbing = false;
        //GrabPointを削除
        Destroy(m_grabObj);
    }
    public void HookShooting()
    {
        DebugPrint.Print(string.Format("AttachmentObj:{0}", AttachmentObj?.name));
        //右クリックで発射
        if (Input.GetMouseButtonDown(1))
        {
            if (!Obirope.isLoaded)
                LaunchHook();
            else
                DetachHook();
        }


    }
    public void RopeChangeLength()
    {
        if (!Obirope.isLoaded) return;
        float Wheel = Input.GetAxis("Mouse ScrollWheel");

        if (Input.GetKey(KeyCode.Space))
        {
            cursor.ChangeLength(Mathf.Clamp(Obirope.restLength - hookExtendRetractSpeed * Time.deltaTime, 1, 20));
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cursor.ChangeLength(Mathf.Clamp(Obirope.restLength + hookExtendRetractSpeed * Time.deltaTime, 1, 20));
        }
        DebugPrint.Print(string.Format("RopeLength{0}", Obirope.restLength));
        cursor.ChangeLength(Mathf.Clamp(Obirope.restLength - hookExtendRetractSpeed * Wheel * Time.deltaTime, 1, 20));
    }
    public void Grab()
    {

    }
    public GameObject Explosion()
    {
        LayerMask layerMask = 1 << LayerMask.NameToLayer("Ropeattach");
        var hits = Physics.SphereCastAll(
            m_tf.position,     //中心
            m_langth,          //半径
            Vector3.forward,   //方向
            0f,                //長さ
            layerMask          //レイヤーマスク
            ).Select(h => h.transform.gameObject).ToList();

        if (!m_cameraChanger.m_is3DCamera)
        {
            hits = hits.Where(_ =>
             {
                 Vector3 Vec = Vector3.ProjectOnPlane(_.transform.position - m_tf.position, Vector3.up);
                 Debug.Log(Mathf.Abs(Vector3.SignedAngle(m_tf.forward, Vec, Vector3.up)));
                 return 90f >= Mathf.Abs(Vector3.SignedAngle(m_tf.forward, Vec, Vector3.up));
             }).ToList();
        }


        GameObject obj = null;
        foreach (var hit in hits)
        {
            //距離を求める
            float Length = Vector3.Distance(m_tf.position, hit.transform.position);
            //attachmentObjとの間にコライダー付きのオブジェクトがあった場合は無視する
            if (Physics.Raycast(m_tf.position, hit.transform.position - m_tf.position, out RaycastHit hitInfo, m_langth))
            {
                //DebugPrint.Print(string.Format("1{0}", hit));
                //DebugPrint.Print(string.Format("2{0}", hitInfo.collider.gameObject));
                if (hit != hitInfo.collider.gameObject)
                {
                    continue;
                }
            }

            //距離が短いなら
            if (MinLength > Length)
            {
                //最短距離を更新
                MinLength = Length;
                //オブジェにエネミーを返す
                obj = hit;
            }

        }
        //距離リセット
        MinLength = 999;

        return obj;

    }
    private void Update()
    {


        AttachmentObj = Explosion();
        //  m_grabMesh.enabled = false;
        if (m_ui != null)
        {
            m_ui.m_attachTf = AttachmentObj?.transform;
        }
    }
}

