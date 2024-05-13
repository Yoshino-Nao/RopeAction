using RootMotion.FinalIK;
using UnityEngine;
public class IKSetup : MonoBehaviour
{
    private FullBodyBipedIK m_ik;
    [SerializeField] private Transform m_rightTarget;
    [SerializeField] private Transform m_leftTarget;
    // Start is called before the first frame update
    void Awake()
    {
        m_ik = GetComponentInChildren<FullBodyBipedIK>();
        m_ik.solver.leftHandEffector.target = m_leftTarget;
        m_ik.solver.rightHandEffector.target = m_rightTarget;
    }
}
