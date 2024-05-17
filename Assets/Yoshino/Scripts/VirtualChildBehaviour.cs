using UnityEngine;

public class VirtualChildBehaviour : MonoBehaviour
{
    // ��̃��[�J�����W��p�����A�^�[�Q�b�g�ւ̑��Έʒu
    private Vector3 _relavivePosition;
    // virtualParent�̃��[�J�����W��p�����AvirtualParent.rotation -> virtualChild.rotation�ւ̉�]��
    private Vector3 _parentLocalAxis;
    // virtualParent.rotation -> virtualChild.rotation�ւ̉�]��
    private float _rotationAngle;

    public Transform VirtualParent { get; private set; } = null;

    public Transform VirtualChild => transform;

    public bool ExistVirtualParent => VirtualParent != null;

    /// <summary>
    /// �e�q�֌W�̂悤�ȐU�镑�������������I�u�W�F�N�g��o�^����
    /// </summary>
    /// <param name="virtualParent"></param>
    /// <returns></returns>
    public void RegisterParent(Transform virtualParent)
    {
        if (ExistVirtualParent)
        {
            Debug.LogWarning($"[VirtualChildBehaviour] '{VirtualChild.name}'�͂��ł�'{VirtualParent.name}'�̎q���Ƃ��ēo�^����Ă��܂��B");
            return;
        }

        VirtualParent = virtualParent;

        // VirtualParent�̃��[�J�����W�n�ŕ\�����ꍇ�̃^�[�Q�b�g�I�u�W�F�N�g�̈ʒu
        _relavivePosition = VirtualParent.InverseTransformPoint(VirtualChild.position);

        // �o�^����̏�Ԃ�VirtualParent��rotation��VirtualChild��rotation�Ɉ�v�����邽�߂̉�]�s������߂�B
        // ���߂�����]�s���A�Ƃ�, VirtualChild, VirtualParent��rotation��C,P�Ƃ���ƁAC = A * P�ł���A
        // ���ӂ���P�̋t�s��iInverse(P)�j��������� A = C * Inverse(P)
        var rotationMatrix = VirtualChild.rotation * Quaternion.Inverse(VirtualParent.rotation);

        // rotationMatrix�̓O���[�o�����W���g���ĕ\������Ă��邽�߁AvirtualParent�������ł���]������͎g���Ȃ����A
        // Quaternion�����]�ʁiangle�j�Ɖ�]���iaxis�j�𒊏o����ƁAangle�͂��̂܂܎g����
        rotationMatrix.ToAngleAxis(out _rotationAngle, out Vector3 grobalAxis);

        // axis��VirtualParent�̃��[�J�����W�ŕ\�����Ă����A
        // �g���^�C�~���O��VirtualParent��rotation�����ƂɃO���[�o�����W�֕ϊ�������
        _parentLocalAxis = VirtualParent.InverseTransformVector(grobalAxis);
    }

    /// <summary>
    /// �o�[�`�����Ȑe�q�֌W��j������
    /// </summary>
    /// <returns></returns>
    public void UnregisterParent()
    {
        VirtualParent = null;
    }

    private void FixedUpdate()
    {
        if (ExistVirtualParent == false) return;

        // ���[�J�����W -> �O���[�o�����W�֕ϊ�
        var position = VirtualParent.TransformPoint(_relavivePosition);
        var grobalAxis = VirtualParent.TransformVector(_parentLocalAxis);

        // (��]�s��) * �e��rotation
        var rotation = Quaternion.AngleAxis(_rotationAngle, grobalAxis) * VirtualParent.rotation;

        VirtualChild.SetPositionAndRotation(position, rotation);
    }
}

