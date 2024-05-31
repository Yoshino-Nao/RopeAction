using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generateball : MonoBehaviour
{
    [SerializeField, Header("���Ԍo�߂ŏo���{�[��")]
    private GameObject ball;

    [SerializeField, Header("�������鎞��")]
    private float count;

    //���݂̎���
    private float NowTimer;

    private void Update()
    {
        if(NowTimer > count)
        {
            Instantiate(ball, transform.position, transform.rotation);
            NowTimer = 0;
        }
        else
        {
            NowTimer += Time.deltaTime;
        }
    }
}
