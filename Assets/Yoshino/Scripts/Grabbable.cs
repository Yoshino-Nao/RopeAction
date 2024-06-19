using Obi;
using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Grabbable : MonoBehaviour
{
    [SerializeField] Transform m_rightTarget;
    [SerializeField] Transform m_leftTarget;

    private VirtualChildBehaviour m_childBehaviour;
    private Collider m_col;
    private ObiColliderBase m_obiCol;
    public ObiColliderBase GetObiCol
    {
        get { return m_obiCol; }
    }
    private Rigidbody m_rb;
    // Start is called before the first frame update
    public void SetUp()
    {
        m_childBehaviour = GetComponent<VirtualChildBehaviour>();
        m_col = GetComponent<Collider>();
        m_obiCol = GetComponent<ObiColliderBase>();
        m_rb = GetComponent<Rigidbody>();
    }
    public void SetArmIKTarget(ref FullBodyBipedIK IK)
    {
        IK.solver.rightHandEffector.target = m_rightTarget;
        IK.solver.leftHandEffector.target = m_leftTarget;
    }
    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// �^���I�Ȑe�q�֌W������
    /// </summary>
    /// <param name="parent"></param>
    public void SetParent(Transform parent)
    {
        if (parent == null)
        {
            m_childBehaviour.UnregisterParent();
        }
        else
        {
            m_childBehaviour.RegisterParent(parent);
        }
    }
    /// <summary>
    /// �������Z��L����
    /// </summary>
    public void EnablePhysics()
    {
        m_col.isTrigger = false;
        m_rb.isKinematic = false;
    }
    public void DisableCollider()
    {
        m_col.isTrigger = true;
        m_rb.isKinematic = true;
    }
}