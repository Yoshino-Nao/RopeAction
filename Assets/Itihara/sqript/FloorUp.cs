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
    public float PosY = 5.0f;//������Y���W

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
