using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Rigidbody>();
    }
    //�Փ˂������Aobject�j��
    void OnCollisionEnter(Collision collision)
    {
        //player�^�O������
        if (collision.gameObject.tag == "Player")
        {
            //��b�������
            Destroy(gameObject,1.0f);
        }
    }
}
