using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RopeGrabInteractable : MonoBehaviour
{
    enum FollowState
    {
        No,
        Follow
    }

    enum GrabState
    {
        No,
        Grab
    }

    [SerializeField] private FollowState followState;

    [SerializeField] private GrabState grabState = GrabState.No;

    [SerializeField] private Rope rope;
    
    private Rigidbody selfRigidbody;

    private Vector3 grabRopePosition;

    private Vector3 grabRopeDirection;

    void Awake()
    {
        this.followState = FollowState.No;
        this.selfRigidbody = GetComponent<Rigidbody>();
        this.rope = this.GetComponentInParent<Rope>();
        if (this.rope != null) this.transform.parent = this.rope?.transform.parent;

    }

    public void Grab()
    {
        this.grabState = GrabState.Grab;
        this.rope.AddOrEnableParticleAttachment(this, this.transform);

    }

    public void Drop()
    {
        this.grabState = GrabState.No;
        this.rope.DisableParticleAttachment(this);
    }
    public void OnObiCollisionEnter(Vector3 ropePoint, Vector3 ropeDirection)
    {
        this.followState = FollowState.Follow;
        SetFollowParameter(ropePoint, ropeDirection);
    }

    public void OnObiCollisionStay(Vector3 ropePoint, Vector3 ropeDirection)
    {
        SetFollowParameter(ropePoint, ropeDirection);
    }

    public void OnObiCollisionExit(Vector3 ropePoint, Vector3 ropeDirection)
    {
        this.followState = FollowState.No;
    }


    private void SetFollowParameter(Vector3 grabRopePosition, Vector3 grabRopeDirection)
    {
        this.grabRopePosition = grabRopePosition;
        this.grabRopeDirection = grabRopeDirection;
    }

    /// <summary>
    /// コントローラーと連動して動かす
    /// </summary>
    private void FixedUpdate()
    {
        if (this.followState == FollowState.Follow && this.grabState == GrabState.No)
        {
            this.selfRigidbody.MovePosition(grabRopePosition);
        }
    }

}
