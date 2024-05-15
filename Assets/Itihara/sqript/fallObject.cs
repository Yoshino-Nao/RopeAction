using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        //player�^�O������
        if (collision.gameObject.tag == "Player")
        {
            body.isKinematic = false;
        }

        if(collision.gameObject.tag =="floor")
        {
            Destroy(this.gameObject,5.0f);
        }
    }
}
