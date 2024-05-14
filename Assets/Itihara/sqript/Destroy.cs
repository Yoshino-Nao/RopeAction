using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Rigidbody>();
    }
    //Õ“Ë‚µ‚½Aobject”j‰ó
    void OnCollisionEnter(Collision collision)
    {
        //playerƒ^ƒO‚ªğŒ
        if (collision.gameObject.tag == "Player")
        {
            //ˆê•bŒãÁ‚¦‚é
            Destroy(gameObject,1.0f);
        }
    }
}
