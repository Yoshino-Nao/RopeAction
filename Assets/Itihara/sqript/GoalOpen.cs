using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoalOpen : MonoBehaviour
{
    Vector3 defaultY;　//初期Y座標
    Vector3 moveY;     //true時の動くY座標
    float speed = 1f;
    float MaxSpeed;  //動くスピード
    public bool isMove;//動くフラグ

    // Start is called before the first frame update
    void Start()
    {
        MaxSpeed = speed * Time.deltaTime;
        defaultY = transform.position;
        moveY = new Vector3(transform.position.x, transform.position.y - 5f, transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveY, MaxSpeed);
        }
        else if (!isMove)
        {
            transform.position = Vector3.MoveTowards(transform.position, defaultY, MaxSpeed);
        }
    }
}
