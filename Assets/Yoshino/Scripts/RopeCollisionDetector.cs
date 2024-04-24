using Obi;
using UnityEngine;

[RequireComponent(typeof(ObiCollider))]
public class RopeCollisionDetector : MonoBehaviour
{
    public bool IsLeft { get; private set; }

    private ObiContactEventDispatcher contactEventDispatcher;


    ObiCollider selfCollider;

    private void Awake()
    {
        this.selfCollider = GetComponent<ObiCollider>();
        this.contactEventDispatcher = FindObjectOfType<ObiContactEventDispatcher>();

        
    }

    private void OnEnable()
    {
        this.contactEventDispatcher.onContactEnter.AddListener(SolverContact_OnCollisionEnter);
        this.contactEventDispatcher.onContactStay.AddListener(SolverContact_OnCollisionStay);
        this.contactEventDispatcher.onContactExit.AddListener(SolverContact_OnExit);
    }

    private void OnDisable()
    {
        this.contactEventDispatcher.onContactEnter.RemoveListener(SolverContact_OnCollisionEnter);
        this.contactEventDispatcher.onContactStay.RemoveListener(SolverContact_OnCollisionStay);
        this.contactEventDispatcher.onContactExit.RemoveListener(SolverContact_OnExit);
    }

    public void SolverContact_OnCollisionEnter(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<Rope>().LeftRopeGrabInteractable.OnObiCollisionEnter(projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<Rope>().RightRopeGrabInteractable.OnObiCollisionEnter(projectPos, ropeDirection);
            }
        });
    }

    public void SolverContact_OnCollisionStay(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<Rope>().LeftRopeGrabInteractable.OnObiCollisionStay(projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<Rope>().RightRopeGrabInteractable.OnObiCollisionStay(projectPos, ropeDirection);
            }
        });
    }

    public void SolverContact_OnExit(ObiSolver sender, Oni.Contact contact)
    {
        AnalyzeContact(sender, contact, (ObiRope obiRope, Vector3 projectPos, Vector3 ropeDirection) =>
        {
            if (this.IsLeft)
            {
                obiRope.GetComponent<Rope>().LeftRopeGrabInteractable.OnObiCollisionExit(projectPos, ropeDirection);
            }
            else
            {
                obiRope.GetComponent<Rope>().RightRopeGrabInteractable.OnObiCollisionExit(projectPos, ropeDirection);
            }
        });
    }

    private void AnalyzeContact(ObiSolver sender, Oni.Contact contact, System.Action<ObiRope, Vector3, Vector3> OnCollisionAction)
    {
        int simplexIndex = sender.simplices[contact.bodyA];
        var particleInActor = sender.particleToActor[simplexIndex];

        var world = ObiColliderWorld.GetInstance();
        var contactCollider = world.colliderHandles[contact.bodyB].owner;

        if ((particleInActor.actor is ObiRope) && contactCollider == selfCollider)
        {
            var obiRope = particleInActor.actor as ObiRope;
            if (obiRope.TryGetNearestParticleIndex(this.transform.position, out var outParticleIndex))
            {
                if (obiRope.TryGetRopeProjectionPosition(this.transform.position, outParticleIndex, sender, out var projectionPosition, out var outRopeDirection))
                {
                    OnCollisionAction?.Invoke(obiRope, projectionPosition, outRopeDirection);
                }
            }
        }
    }
}


