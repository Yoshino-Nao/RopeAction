using RootMotion.FinalIK;
using UnityEngine;

public class IKTarget : MonoBehaviour
{
    [SerializeField] Transform m_rightTarget;
    [SerializeField] Transform m_leftTarget;
    private Transform m_tf => transform;
    private void Awake()
    {
        var IK = m_tf.parent.GetComponentInChildren<FullBodyBipedIK>();
        IK.solver.rightHandEffector.target = m_rightTarget;
        IK.solver.leftHandEffector.target = m_leftTarget;
    }
    public void Move(Vector3 target)
    {
        m_tf.position = target;
    }
}
