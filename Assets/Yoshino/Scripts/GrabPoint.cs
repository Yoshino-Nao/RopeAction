using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GrabPoint : MonoBehaviour
{
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

    // Update is called once per frame
    void Update()
    {

    }
    /// <summary>
    /// ‹^—“I‚ÈeqŠÖŒW‚ğ‚Â‚­‚é
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
    /// •¨—‰‰Z‚ğ—LŒø‰»
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
