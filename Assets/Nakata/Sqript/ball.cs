using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField, Header("����")]
    private float timer;

    Generateball Generateball;
    void Start()
    {
        Generateball = GetComponent<Generateball>();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    //�n�ʂɐG�ꂽ��
    //    if (collision.gameObject.name == "field1")
    //    {
    //        //�H�b��ɃI�u�W�F�N�g���폜
    //        Destroy(this.gameObject, timer);
    //    }
    //}

    ////�I�u�W�F�N�g���폜���ꂽ��
    //private void OnDestroy()
    //{
    //    Generateball.Generate();
    //}
}
