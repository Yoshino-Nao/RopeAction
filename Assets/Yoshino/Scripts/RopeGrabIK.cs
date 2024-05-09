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
    [SerializeField] private CapsuleCollider m_playerCap;
    private Transform m_tf;
    private ObiSolver m_solver;
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
        //m_posA = m_actor.GetParticlePosition(index);
        //m_posB = m_actor.GetParticlePosition(index + m_obiRope.activeParticleCount - 2);
    }
    private void OnDrawGizmos()
    {

    }
    // Update is called once per frame
    void FixedUpdate()
    {


        //DebugPrint.Print(string.Format("{0}", m_ropeParticleGetter.position));
        if (m_hookShot.GetisLoaded)
        {
            Vector3 WorldPos = m_ropeParticleGetter.position;
            Vector3 PlayerCenter = m_playerTf.position + m_playerCap.center + m_playerCap.center;
            WorldPos = PlayerCenter + (m_hookShot.GetAttachmentObj.transform.position - PlayerCenter).normalized * radius;
            if (m_obiRope.TryGetNearestParticleIndex(WorldPos, out var outParticleIndex))
            {
                DebugPrint.Print(string.Format("Index:{0}", outParticleIndex));
                //if (outParticleIndex < 390) return;
                if (m_obiRope.TryGetRopeProjectionPosition(WorldPos, outParticleIndex, m_solver, out var projectionPosition, out var outRopeDirection))
                {
                    m_tf.position = projectionPosition;
                    m_tf.rotation = Quaternion.LookRotation(-outRopeDirection);

                    Debug.DrawRay(m_tf.position, -outRopeDirection);
                }
            }
        }
    }
}
