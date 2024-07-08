using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Switch : MonoBehaviour
{
    Vector3 bottomY;
    float speed = 0.5f;
    Vector3 current;
    float MaxSpeed;
    bool active = false;
    public GoalOpen goal;
    // Start is called before the first frame update
    void Start()
    {
        MaxSpeed = speed * Time.deltaTime;
        current = transform.position;
        bottomY= new Vector3(transform.position.x,transform.position.y - 0.5f,transform.position.z);
    }
    
    // Update is called once per frame
    void Update()
    {

        if (active)
        {
            //transform.position -= Vector3.up * speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, bottomY, MaxSpeed);
        }
        else
        {
            //transform.position += Vector3.up * speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position,current,MaxSpeed);
        }
        goal.isMove = Mathf.Abs(transform.position.y - bottomY.y) <= 0.01f;

    }
    private void OnTriggerEnter(Collider other)
    {
        if (!active && other.CompareTag("Carry"))
        {
            active = true;
            
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (active && other.CompareTag("Carry"))
        {
            active = false;
        }
    }
}
