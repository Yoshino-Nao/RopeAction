using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoalOpen : MonoBehaviour
{
    Vector3 GoalY;　//初期Y座標
    Vector3 moveY;     //true時の動くY座標
    float speed = 1f;
    float MaxSpeed;  //動くスピード
    public float PosY = 10.0f;//動かすY座標
    public bool isMove;//動くフラグ

    // Start is called before the first frame update
    void Start()
    {
        GoalY = transform.position;
        moveY = new Vector3(transform.position.x, transform.position.y - 10f, transform.position.z);
        isMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        MaxSpeed = speed * Time.deltaTime;
        if (isMove == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveY, MaxSpeed);
        }
        else if (isMove == false) 
        {
            transform.position = Vector3.MoveTowards(transform.position, GoalY, MaxSpeed);
        }
        //if (isMove == false)
        //{
        //    Debug.Log("false");
        //}
        //else
        //{
        //    Debug.Log("true");
        //}
    }
}
