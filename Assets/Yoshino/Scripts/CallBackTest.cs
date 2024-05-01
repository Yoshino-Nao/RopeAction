using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallBackTest : MonoBehaviour
{
    [SerializeField] private RopeGrabInteractable ropeInteractable;
    [SerializeField] private bool Grab = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Grab)
        {
            ropeInteractable.Grab();
        }
        else
        {

            ropeInteractable.Drop();
        }
    }

}
