using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField, Header("����")]
    private float timer;

    private void OnCollisionEnter(Collision collision)
    {
        //�n�ʂɐG�ꂽ��
        if (collision.gameObject.layer == 3)
        {
            Destroy(this.gameObject, timer);
        }
    }
}
