using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    //初期ターゲット
    private int point = 0;
    [SerializeField, Header("移動速度")]
    private float moveSpeed;
    [SerializeField, Header("移動ルート")]
    private Transform []target;
    [SerializeField, Header("クールタイム")]
    private float Timer;
    private float nowTimer;

    void FixedUpdate()
    {
        if(Timer > nowTimer)
        {
            nowTimer += Time.deltaTime;
        }
        else
        {
            //目的地まで移動する処理
            transform.position = Vector3.MoveTowards(transform.position, target[point].position, moveSpeed * Time.deltaTime);
            //目的地に到着したら
            if (transform.position == target[point].position)
            {
                //ターゲットを次の地点に変更
                point++;
                nowTimer = 0;
            }
            //最終目的地に到達したら移動をループする
            if (point == target.Length)
            {
                point = 0;
            }
        }
    }
}
