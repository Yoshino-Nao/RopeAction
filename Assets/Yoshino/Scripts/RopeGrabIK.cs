using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class RopeGrabIK : MonoBehaviour
{
    [SerializeField] private ObiRope m_obiRope;
    [SerializeField] private Transform m_hookTf;
    [SerializeField] private Transform m_playerTf;

    private Transform m_tf;
    private ObiSolver m_solver;
    private ObiActor m_actor;
    [SerializeField] private HookShot m_hookShot;
    [SerializeField] private Transform m_ropeParticleGetter;

    public float radius = 0.5f;
    [Range(150, 300)]
    public int Count = 185;
    int index = 185;
    private Vector3 m_posA;
    private Vector3 m_posB;
    private Vector3 m_posC;
    [Button]
    void DebugButton()
    {

    }
    private void Start()
    {
        m_tf = transform;
        m_solver = GetComponentInParent<ObiSolver>();
        m_actor = GetComponent<ObiActor>();
        //m_posA = m_actor.GetParticlePosition(index);
        //m_posB = m_actor.GetParticlePosition(index + m_obiRope.activeParticleCount - 2);
    }
    private void OnDrawGizmos()
    {
        
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_hookShot.GetisLoaded)
        {
            if (m_obiRope.TryGetNearestParticleIndex(m_ropeParticleGetter.position, out var outParticleIndex))
            {
                if (m_obiRope.TryGetRopeProjectionPosition(m_ropeParticleGetter.position, outParticleIndex, m_solver, out var projectionPosition, out var outRopeDirection))
                {
                    m_tf.position = projectionPosition;
                    m_tf.rotation = Quaternion.LookRotation(-outRopeDirection);

                    Debug.DrawRay(m_tf.position, -outRopeDirection);
                }
            }
        }
    }
}
