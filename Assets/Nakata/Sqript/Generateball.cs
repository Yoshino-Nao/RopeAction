using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generateball : MonoBehaviour
{
    [SerializeField, Header("時間経過で出すボール")]
    private GameObject ball;

    [SerializeField, Header("生成する時間")]
    private float count;

    //現在の時間
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
