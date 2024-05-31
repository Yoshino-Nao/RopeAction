using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ball : MonoBehaviour
{
    [SerializeField, Header("éûä‘")]
    private float timer;

    private void OnCollisionEnter(Collision collision)
    {
        //ínñ Ç…êGÇÍÇΩÇÁ
        if (collision.gameObject.layer == 3)
        {
            Destroy(this.gameObject, timer);
        }
    }
}
