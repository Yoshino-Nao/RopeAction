using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fallObject : MonoBehaviour
{
   
    Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    //�Փ˂������Aobject�j��
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "floor")
        {
            if(!body.isKinematic)
            body.isKinematic = true;
        }
    }
    public void Fall()
    {
        body.isKinematic = false;
    }
}