using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generateball : MonoBehaviour
{
    [SerializeField, Header("時間経過で出すボール")]
    private GameObject ball;

    //[SerializeField, Header("ボールの生成場所")]
    //private GameObject spawnpoint;

    //[SerializeField, Header("生成タイム")]
    //private float timer;

    //static float time = 0;


    public void Generate()
    {
        Instantiate(ball, transform.position, transform.rotation);
    }
}
