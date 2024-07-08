using Obi;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SetChildOnHit : MonoBehaviour
{
    private ObiSolver m_obiSolver;

    private Transform m_tf => transform;
    private BoxCollider m_col;

    private VirtualChildBehaviour m_virtualChildBehaviour;
    private MoveTest m_player;
    private void Start()
    {
        m_obiSolver = GetComponentInParent<ObiSolver>();
        m_col = GetComponent<BoxCollider>();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.transform.parent = transform;
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.transform.parent = transform;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.transform.parent = m_obiSolver.transform;
        }
    }

}
