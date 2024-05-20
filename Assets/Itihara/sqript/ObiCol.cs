using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using Obi;


[RequireComponent(typeof(ObiSolver))]
public class ObiCol : MonoBehaviour
{
    ObiSolver solver;

    public fallObject[] fallObject;
    private void Awake()
    {
        solver = GetComponent<ObiSolver>();
    }
    private void OnEnable()
    {
        solver.OnCollision += Solver_OnCollision;
    }

    private void OnDisable()
    {
        solver.OnCollision -= Solver_OnCollision;
    }
    private void Solver_OnCollision(ObiSolver s, ObiSolver.ObiCollisionEventArgs e)
    {
        var world = ObiColliderWorld.GetInstance();
        foreach (Oni.Contact contact in e.contacts)
        {
            var col = world.colliderHandles[contact.bodyB].owner;
            if (col != null)
            {
                var fallObject = col.GetComponent<fallObject>();
                if (fallObject != null)
                    fallObject.Fall();
            }
        }
    }
    public ObiRope rope;
    public float tensionThreshold = 10.0f; // à¯Ç¡í£ÇËÇ∆îªíËÇ∑ÇÈóÕÇÃËáíl
    public Rigidbody targetRigidbody; // ëÄçÏëŒè€ÇÃRigidbody

    void Update()
    {
        if (IsRopeUnderTension())
        {
            if (targetRigidbody != null)
            {
                targetRigidbody.isKinematic = false;
                Debug.Log("Rope is under tension! isKinematic is set to false.");
            }
        }
    }

    bool IsRopeUnderTension()
    {
        var solver = rope.solver;

        for (int i = 0; i < rope.activeParticleCount; i++)
        {
            int particleIndex = rope.solverIndices[i];
            Vector3 force = solver.externalForces[particleIndex];
            if (force.magnitude > tensionThreshold)
            {
                return true;
            }
        }
        return false;
    }
}