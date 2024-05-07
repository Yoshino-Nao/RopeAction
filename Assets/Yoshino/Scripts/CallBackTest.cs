using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class CallBackTest : MonoBehaviour
{
    [SerializeField] private RopeGrabInteractable ropeInteractable;
    [SerializeField] private bool Grab = false;
    [Button]
    void GrabTest()
    {
        ropeInteractable.Grab();
    }
    [Button]
    void DropTest() 
    {
        ropeInteractable.Drop();
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        //if (Grab)
        //{
        //}
        //else
        //{

        //    ropeInteractable.Drop();
        //}
    }

}
