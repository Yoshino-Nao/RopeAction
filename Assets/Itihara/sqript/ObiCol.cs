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
            if (col != null )
            {
                var fallObject = col.GetComponent<fallObject>();
                if (fallObject != null)
                    fallObject.IsRopeUnderTension();
            }
        }
    }
    //public ObiRope rope;
    //public float tensionThreshold = 10.0f; // ˆø‚Á’£‚è‚Æ”»’è‚·‚é—Í‚Ìè‡’l
    //public Rigidbody targetRigidbody; // ‘€ì‘ÎÛ‚ÌRigidbody
    //private float time = 0.0f;
    //private bool check;

    //bool IsRopeUnderTension()
    //{
    //    time += UnityEngine.Time.deltaTime;
    //    var solver = rope.solver;
    //    float tension = rope.CalculateLength() / rope.restLength - 1;
    //    //DebugPrint.Print(string.Format("{0}", tension));

    //    int particleIndex = rope.solverIndices[i];
    //    if (tension > tensionThreshold)
    //    {
    //        time += UnityEngine.Time.deltaTime;
    //        DebugPrint.Print(string.Format("{0}", time));
    //        if (time >= 0.5f)
    //            check = true;
    //        if (check)
    //            return true;
    //    }

    //    return false;
    //}
    void Update()
    {
       
       
           
    }
}