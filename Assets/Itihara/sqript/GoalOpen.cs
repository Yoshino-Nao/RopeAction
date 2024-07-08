using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GoalOpen : MonoBehaviour
{
    Vector3 GoalY;�@//����Y���W
    Vector3 moveY;     //true���̓���Y���W
    float speed = 1f;
    float MaxSpeed;  //�����X�s�[�h
    public bool isMove;//�����t���O

    // Start is called before the first frame update
    void Start()
    {
        MaxSpeed = speed * Time.deltaTime;
        GoalY = transform.position;
        moveY = new Vector3(transform.position.x, transform.position.y - 10f, transform.position.z);
        isMove = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMove == true)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveY, MaxSpeed);
        }
        else if (isMove == false) 
        {
            transform.position = Vector3.MoveTowards(transform.position, GoalY, MaxSpeed);
        }
        if (isMove == false)
        {
            Debug.Log("false");
        }
        else
        {
            Debug.Log("true");
        }
    }
}
