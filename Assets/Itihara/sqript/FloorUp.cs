using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FloorUp : MonoBehaviour
{
    Vector3 GoalY;　//初期Y座標
    Vector3 moveY;     //true時の動くY座標
    float speed = 1f;
    float MaxSpeed;  //動くスピード
    public bool isMove;//動くフラグ
    public float PosY = 5.0f;//動かすY座標

    // Start is called before the first frame update
    void Start()
    {
        GoalY = transform.position;
        moveY = new Vector3(transform.position.x, transform.position.y + PosY, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        MaxSpeed = speed * Time.deltaTime;
        if (isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveY, MaxSpeed);
        }
        else if (!isMove)
        {
           
            transform.position = Vector3.MoveTowards(transform.position, GoalY, MaxSpeed);
        }
    }
}
