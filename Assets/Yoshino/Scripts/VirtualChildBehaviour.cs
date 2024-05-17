using UnityEngine;

public class VirtualChildBehaviour : MonoBehaviour
{
    // 手のローカル座標を用いた、ターゲットへの相対位置
    private Vector3 _relavivePosition;
    // virtualParentのローカル座標を用いた、virtualParent.rotation -> virtualChild.rotationへの回転軸
    private Vector3 _parentLocalAxis;
    // virtualParent.rotation -> virtualChild.rotationへの回転量
    private float _rotationAngle;

    public Transform VirtualParent { get; private set; } = null;

    public Transform VirtualChild => transform;

    public bool ExistVirtualParent => VirtualParent != null;

    /// <summary>
    /// 親子関係のような振る舞いをさせたいオブジェクトを登録する
    /// </summary>
    /// <param name="virtualParent"></param>
    /// <returns></returns>
    public void RegisterParent(Transform virtualParent)
    {
        if (ExistVirtualParent)
        {
            Debug.LogWarning($"[VirtualChildBehaviour] '{VirtualChild.name}'はすでに'{VirtualParent.name}'の子供として登録されています。");
            return;
        }

        VirtualParent = virtualParent;

        // VirtualParentのローカル座標系で表した場合のターゲットオブジェクトの位置
        _relavivePosition = VirtualParent.InverseTransformPoint(VirtualChild.position);

        // 登録直後の状態のVirtualParentのrotationをVirtualChildのrotationに一致させるための回転行列を求める。
        // 求めたい回転行列をAとし, VirtualChild, VirtualParentのrotationをC,Pとすると、C = A * Pであり、
        // 両辺からPの逆行列（Inverse(P)）をかけると A = C * Inverse(P)
        var rotationMatrix = VirtualChild.rotation * Quaternion.Inverse(VirtualParent.rotation);

        // rotationMatrixはグローバル座標を使って表現されているため、virtualParentが少しでも回転した後は使えないが、
        // Quaternionから回転量（angle）と回転軸（axis）を抽出すると、angleはそのまま使える
        rotationMatrix.ToAngleAxis(out _rotationAngle, out Vector3 grobalAxis);

        // axisはVirtualParentのローカル座標で表現しておき、
        // 使うタイミングでVirtualParentのrotationをもとにグローバル座標へ変換し直す
        _parentLocalAxis = VirtualParent.InverseTransformVector(grobalAxis);
    }

    /// <summary>
    /// バーチャルな親子関係を破棄する
    /// </summary>
    /// <returns></returns>
    public void UnregisterParent()
    {
        VirtualParent = null;
    }

    private void FixedUpdate()
    {
        if (ExistVirtualParent == false) return;

        // ローカル座標 -> グローバル座標へ変換
        var position = VirtualParent.TransformPoint(_relavivePosition);
        var grobalAxis = VirtualParent.TransformVector(_parentLocalAxis);

        // (回転行列) * 親のrotation
        var rotation = Quaternion.AngleAxis(_rotationAngle, grobalAxis) * VirtualParent.rotation;

        VirtualChild.SetPositionAndRotation(position, rotation);
    }
}

