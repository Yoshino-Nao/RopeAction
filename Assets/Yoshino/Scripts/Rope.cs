using Obi;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private ObiRope obiRope;
    Transform obiRope_Tf;
    public RopeGrabInteractable LeftRopeGrabInteractable => this.LeftRopeGrabInteractable;
    public RopeGrabInteractable RightRopeGrabInteractable => this.RightRopeGrabInteractable;

    Dictionary<RopeGrabInteractable, ObiParticleAttachment>
        attachimentDict = new Dictionary<RopeGrabInteractable, ObiParticleAttachment>();
    // Start is called before the first frame update
    void Awake()
    {
        if (obiRope != null)
        {
            obiRope.GetComponent<ObiRope>();
        }


        obiRope_Tf = obiRope.transform;
       
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (obiRope != null)
        {
            //Debug.Log()
        }
    }
    private void OnDrawGizmos()
    {
       
    }
    public void AddOrEnableParticleAttachment(RopeGrabInteractable ropeGrabInteractable, Transform target)
    {
        if (!this.attachimentDict.ContainsKey(ropeGrabInteractable))
        {
            var particleAttachment = this.AddComponent<ObiParticleAttachment>();
            particleAttachment.target = target;

            particleAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
            particleAttachment.particleGroup = FindNearObiParticleGroup(target);
            this.attachimentDict[ropeGrabInteractable] = particleAttachment;
        }
        else
        {
            this.attachimentDict[ropeGrabInteractable].particleGroup = FindNearObiParticleGroup(target);
            this.attachimentDict[ropeGrabInteractable].enabled = true;
        }
    }
    public void DisableParticleAttachment(RopeGrabInteractable ropeGrabInteractable)
    {
        if (this.attachimentDict.ContainsKey(ropeGrabInteractable))
        {
            this.attachimentDict[ropeGrabInteractable].enabled = false;
        }
    }
    private ObiParticleGroup FindNearObiParticleGroup(Transform target)
    {
        float distance = 100000f;
        ObiParticleGroup findParticleGroup = null;
        foreach (var group in obiRope.blueprint.groups)
        {
            foreach (var particleindex in group.particleIndices)
            {
                //ObiRopeExtensionéQè∆
                var particlePosition = obiRope.GetParticlePosition(particleindex);
                var currentDistance = Vector3.Distance(target.position, particlePosition);
                if (currentDistance <= distance)
                {
                    distance = currentDistance;
                    findParticleGroup = group;
                }
            }
        }

        return findParticleGroup;
    }
}
