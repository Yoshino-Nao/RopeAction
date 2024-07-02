using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class FloorUp : MonoBehaviour
{
    Vector3 GoalY;�@//����Y���W
    Vector3 moveY;     //true���̓���Y���W
    float speed = 1f;
    float MaxSpeed;  //�����X�s�[�h
    public bool isMove;//�����t���O
    public float MSpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        MaxSpeed = speed * Time.deltaTime;
        GoalY = transform.position;
        moveY = new Vector3(transform.position.x, transform.position.y + MSpeed, transform.position.z);
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
           
            transform.position = Vector3.MoveTowards(transform.position, GoalY, MaxSpeed);
        }
    }
}
