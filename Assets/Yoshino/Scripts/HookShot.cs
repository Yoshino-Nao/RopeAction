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
	 * シーンに対してレイキャストして、フックを何かに取り付けられるかどうかを確認します。
	 */
    private void LaunchHook()
    {

        //このオブジェクトと同じ XY 平面内のシーン内のマウスの位置を取得します。
        Vector3 mouse = Input.mousePosition;
        mouse.z = transform.position.z - Camera.main.transform.position.z;
        Vector3 mouseInScene = Camera.main.ScreenToWorldPoint(mouse);

        //キャラクターからマウス座標へのRayを取得します。
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

        //Pin Constraintsをクリア
        pinConstraints = rope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
        pinConstraints.Clear();
        //ヒットした地点の座標をローカルの座標に変換
        Vector3 localHit = rope.transform.InverseTransformPoint(hookAttachment.point);
        //ロープ パスを手順に従って生成します (時間の経過とともに延長するため、短いセグメントのみ)。
        int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
        blueprint.path.Clear();
        blueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
        blueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
        blueprint.path.FlushEvents();

        //ロープのパーティクル表現を生成します (完了するまで待ちます)。
        yield return blueprint.Generate();

        //ブループリントを設定します(これにより、パーティクル/コンストレイントがソルバーに追加され、それらのシミュレーションが開始されます)。
        rope.ropeBlueprint = blueprint;

        //1フレーム待ちます
        yield return null;

        rope.GetComponent<MeshRenderer>().enabled = true;

        //ロープを伸ばすときに位置を上書きするので、質量をゼロに設定します。
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 0;
        float currentLength = 0;

        //最後のパーティクルがヒットした地点まで到達していない間、ロープを延長します。
        while (true)
        {
            //solverspaceでロープの起点を計算する
            Vector3 origin = solver.transform.InverseTransformPoint(rope.transform.position);

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
            for (int i = 0; i < rope.elements.Count; ++i)
            {
                solver.positions[rope.elements[i].particle1] = origin + direction * length;
                solver.positions[rope.elements[i].particle2] = origin + direction * (length + rope.elements[i].restLength);
                length += rope.elements[i].restLength;
            }

            //1フレーム待ちます
            yield return null;
        }

        //ロープが配置された時点でシミュレーションが引き継がれるように質量を復元します。
        for (int i = 0; i < rope.activeParticleCount; ++i)
            solver.invMasses[rope.solverIndices[i]] = 10; // 1/0.1 = 10

        //ロープの両端をピンで固定します (これにより、キャラクターとロープの間の双方向のインタラクションが可能になります)。
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
