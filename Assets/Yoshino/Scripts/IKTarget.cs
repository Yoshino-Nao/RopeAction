using UnityEngine;

public class IKTarget : MonoBehaviour
{
    [SerializeField] Transform m_rightTarget;
    public Transform GetRightTarget
    {
        get { return m_rightTarget; }
    }
    [SerializeField] Transform m_leftTarget;

}
