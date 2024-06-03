using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetChildOnHit : MonoBehaviour
{
    private ObiSolver m_obiSolver;
    private VirtualChildBehaviour m_virtualChildBehaviour;
    private MoveTest m_player;
    private void Start()
    {
        m_obiSolver = GetComponentInParent<ObiSolver>();
    }

    private void OnCollisionEnter(Collision collision)
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
