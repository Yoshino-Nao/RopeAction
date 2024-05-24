using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField, Header("時間")]
    private float timer;

    Generateball Generateball;
    void Start()
    {
        Generateball = GetComponent<Generateball>();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //地面に触れたら
    //    if (collision.gameObject.name == "field1")
    //    {
    //        //？秒後にオブジェクトを削除
    //        Destroy(this.gameObject, timer);
    //    }
    //}

    ////オブジェクトが削除された時
    //private void OnDestroy()
    //{
    //    Generateball.Generate();
    //}
}
