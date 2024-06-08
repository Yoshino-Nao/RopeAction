using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoalOpen : MonoBehaviour
{
    Vector3 defaultY;�@//����Y���W
    Vector3 moveY;     //true���̓���Y���W
    float speed = 1f;
    float MaxSpeed;  //�����X�s�[�h
    public bool isMove;//�����t���O

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
