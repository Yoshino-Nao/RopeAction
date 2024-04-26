using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallBackTest : MonoBehaviour
{
    public ObiContactEventDispatcher eventDispatcher;
    public RopeCollisionDetector collisionDetector;
    [SerializeField] private RopeGrabInteractable ropeInteractable;
    // Start is called before the first frame update
    void Start()
    {
        eventDispatcher = FindObjectOfType<ObiContactEventDispatcher>();
        collisionDetector = GetComponent<RopeCollisionDetector>();
        //eventDispatcher.onContactEnter.Invoke();

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.T))
        {
            ropeInteractable.Grab();
        }
        //else
        //{

        //    ropeInteractable.Drop();
        //}
    }

}
