using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;
using System.Linq;
using Unity.VisualScripting;

public class HookShot : MonoBehaviour
{
    [SerializeField] private float m_length = 10f;
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
    private Camera m_mainCamera;
    [SerializeField] private UITest m_ui;
    private ObiRope m_rope;
    [SerializeField] private MeshRenderer m_grabMesh;
    private ObiRopeBlueprint blueprint;
    private ObiRopeCursor cursor;
    ObiConstraints<ObiPinConstraintsBatch> pinConstraints;
    [HideInInspector] public ObiStitcher m_obiStitcher;

    private RaycastHit hookAttachment;

    private GameObject m_attachmentTargetObj = null;
    public ObiColliderBase GetAttachmentTargetObiCol
    {
        get { return m_attachmentTargetObj.GetComponent<ObiColliderBase>(); }
    }
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
    private Player m_player;

    [SerializeField] private Grabbable m_grabPoint;
    private GameObject m_grabObj;
    [SerializeField] private ObiParticleAttachment m_particleAttachment;
    [SerializeField] private ObiColliderBase Test;
    [Button]
    void ConnectTest()
    {
        ConnectCurrentObjToOtherObj(Test);
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

    void Awake()
    {
        if (solver == null)
        {
            solver = GetComponentInParent<ObiSolver>();
        }


        m_tf = transform;
        m_mainCamera = Camera.main;
        m_player = FindObjectOfType<Player>();
        m_grabMesh.enabled = false;
        // Create both the rope and the solver:	
        //rope = gameObject.AddComponent<ObiRope>();
        m_rope = gameObject.GetComponent<ObiRope>();

        // Setup a blueprint for the rope:
        blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
        blueprint.resolution = 0.5f;
        blueprint.pooledParticles = particlePoolSize;

        // Tweak rope parameters:
        //rope.maxBending = 0.02f;

        // Add a cursor to be able to change rope length:
        cursor = m_rope.gameObject.GetComponent<ObiRopeCursor>();
        //cursor.cursorMu = 0;
        //cursor.direction = true;
        m_obiStitcher = m_tf.parent.GetComponentInChildren<ObiStitcher>();
        m_particleAttachment = GetComponent<ObiParticleAttachment>();
        m_grabObj = Instantiate(m_grabPoint.gameObject, m_tf.position, Quaternion.identity, solver.transform);
        //m_player.SetGrabPoint = m_grabObj.GetComponent<GrabPoint>();
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
            StartCoroutine(AttachHookForKinematic());
            //if (hookAttachment.rigidbody.isKinematic)
            //{
            //    // We actually hit something, so attach the hook!
                
            //}
            //else
            //{
            //    StartCoroutine(AttachHookForNotKinematic());
            //}

        }

    }

    private IEnumerator AttachHookForKinematic()
    {
        //1フレーム待つ
        yield return null;

        //Pin Constraintsをクリア
        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();

        //ヒットした地点の座標をローカルの座標に変換
        Vector3 localHit = m_rope.transform.InverseTransformPoint(hookAttachment.point);

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

        PlayerGrabs();
        m_grabObj.SetActive(true);
        //m_player.GrabPointSetUp();
    }
    private IEnumerator AttachHookForNotKinematic()
    {
        //1フレーム待つ
        yield return null;

        //Pin Constraintsをクリア
        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        //ヒットした地点の座標をローカルの座標に変換
        Vector3 localHit = m_rope.transform.InverseTransformPoint(hookAttachment.point);
        //ロープ パスを手順に従って生成します (時間の経過とともに延長するため、短いセグメントのみ)。
        int filterEverything = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        int filterNothing = ObiUtils.MakeFilter(ObiUtils.CollideWithNothing, 0);

        blueprint.path.Clear();
        float mass = 0.1f;

        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filterNothing, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit - localHit.normalized, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filterEverything, Color.white, "Hook end 1");

        blueprint.path.AddControlPoint(localHit, Vector3.zero, Vector3.zero, Vector3.up, mass, 0.1f, 1, filterNothing, Color.white, "Hook end 2");
        blueprint.path.FlushEvents();

        //ロープのパーティクル表現を生成します (完了するまで待ちます)。
        yield return blueprint.Generate();

        //ブループリントを設定します(これにより、パーティクル/コンストレイントがソルバーに追加され、それらのシミュレーションが開始されます)。
        m_rope.ropeBlueprint = blueprint;

        //1フレーム待ちます
        //yield return null;

        m_rope.GetComponent<MeshRenderer>().enabled = true;
        m_grabObj = Instantiate(m_grabPoint.gameObject, m_tf.position, Quaternion.identity, solver.transform);
        //m_player.SetGrabPoint = m_grabObj.GetComponent<GrabPoint>();
        //m_player.GrabPointSetUp();

        m_particleAttachment.target = m_grabObj.transform;
        m_particleAttachment.particleGroup = blueprint.groups[0];
        m_particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;

        var target2 = m_tf.AddComponent<ObiParticleAttachment>();
        target2.target = hookAttachment.transform;
        target2.particleGroup = blueprint.groups[2];
        target2.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
    }
    /// <summary>
    /// ロープとオブジェクトを繋ぐ
    /// </summary>
    public void ConnectCurrentObjToOtherObj(ObiColliderBase collider)
    {
        if (m_currentAttachObiCol == null) return;

        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(
            m_rope.elements[0].particle1,
            collider,
            Vector3.zero,
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.AddConstraint(
            m_rope.elements[m_rope.elements.Count - 1].particle2,
            m_currentAttachObiCol.GetComponent<ObiColliderBase>(),
            hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point),
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        m_rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
    }

    public void PlayerGrabs()
    {
        pinConstraints = m_rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        var batch = new ObiPinConstraintsBatch();
        batch.AddConstraint(
            m_rope.elements[0].particle1,
            character,
            m_tf.localPosition,
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );
        batch.AddConstraint(
            m_rope.elements[m_rope.elements.Count - 1].particle2,
            hookAttachment.collider.GetComponent<ObiColliderBase>(),
            hookAttachment.collider.transform.InverseTransformPoint(hookAttachment.point),
            Quaternion.identity,
            0, 0, float.PositiveInfinity
            );

        batch.activeConstraintCount = 2;
        pinConstraints.AddBatch(batch);
        m_rope.SetConstraintsDirty(Oni.ConstraintType.Pin);
        SetGrabMesh(true);
    }
    /// <summary>
    /// Hookの当たり判定をなくす
    /// </summary>
    public void ReleaseRope()
    {

    }

    public void DetachHook()
    {
        SetGrabMesh(false);
        // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
        m_rope.ropeBlueprint = null;
        m_rope.GetComponent<MeshRenderer>().enabled = false;
        m_player.m_isGrabbing = false;
        //GrabPointを削除
        m_grabObj.SetActive(false);
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
        //スペースキーで長さを縮小
        if (Input.GetKey(KeyCode.Space))
        {
            cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Time.deltaTime, min, max));
        }
        
        //シフトキーで長さを延長
        if (Input.GetKey(KeyCode.LeftShift))
        {
            cursor.ChangeLength(Mathf.Clamp(m_rope.restLength + hookExtendRetractSpeed * Time.deltaTime, min, max));
        }
        DebugPrint.Print(string.Format("RopeLength{0}", m_rope.restLength));
        //マウスホイールで長さを変更
        cursor.ChangeLength(Mathf.Clamp(m_rope.restLength - hookExtendRetractSpeed * Wheel * Time.deltaTime, min, max));
    }
    public void SetGrabMesh(bool isEnabled)
    {
        m_grabMesh.enabled = isEnabled;
        if (isEnabled)
        {

            m_obiStitcher.Actor1 = m_rope;
        }

        m_obiStitcher.enabled = isEnabled;
    }
    public GameObject Explosion()
    {
        Vector3 Origin = m_player.transform.position + Vector3.up * 0.1f;
        LayerMask layerMask = LayerMask.NameToLayer("Ropeattach");
        var hits = Physics.SphereCastAll(
            Origin,     //中心
            m_length,          //半径
            Vector3.forward,   //方向
            0f,                //長さ
            1 << layerMask          //レイヤーマスク
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
            foreach (var hit in hits)
            {
                Vector3 ViewPort = m_mainCamera.WorldToViewportPoint(hit.transform.position);
                //距離を求める
                float Length = Vector2.Distance(new Vector2(0.5f, 0.5f), new Vector2(ViewPort.x, ViewPort.y));
                //attachmentObjとの間にコライダー付きのオブジェクトがあった場合は無視する
                if (Physics.Raycast(Origin, hit.transform.position - Origin, out RaycastHit hitInfo, m_length))
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


        m_attachmentTargetObj = Explosion();
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

