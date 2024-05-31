using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField, Header("時間")]
    private float timer;

    private void OnCollisionEnter(Collision collision)
    {
        //地面に触れたら
        if (collision.gameObject.layer == 3)
        {
            Destroy(this.gameObject, timer);
        }
    }
}
