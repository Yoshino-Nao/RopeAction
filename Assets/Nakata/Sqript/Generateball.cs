using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generateball : MonoBehaviour
{
    [SerializeField, Header("���Ԍo�߂ŏo���{�[��")]
    private GameObject ball;

    //[SerializeField, Header("�{�[���̐����ꏊ")]
    //private GameObject spawnpoint;

    //[SerializeField, Header("�����^�C��")]
    //private float timer;

    //static float time = 0;


    public void Generate()
    {
        Instantiate(ball, transform.position, transform.rotation);
    }
}
