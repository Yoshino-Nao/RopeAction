using Obi;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private ObiRope obiRope;
    [SerializeField] ObiActorBlueprint blueprint;
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
        if (blueprint != null)
        {
            Debug.Log("パーティクルの数:" + blueprint.particleCount);
            for (int i = 0; blueprint.particleCount >= i; i++)
            {
                Debug.Log(blueprint.GetParticlePosition(i));
                if (i > blueprint.particleCount)
                {
                    break;
                }
            }
        }
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
        if (obiRope == null || blueprint == null) return;
        for (int i = 0; blueprint.particleCount >= i; i++)
        {
            obiRope_Tf = obiRope.transform;

            Gizmos.DrawSphere((obiRope_Tf.position + blueprint.GetParticlePosition(i)), 0.1f);
            if (i > blueprint.particleCount)
            {
                break;
            }
        }
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
                //ObiRopeExtension参照
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
