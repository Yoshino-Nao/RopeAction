using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    [SerializeField] float m_speed;
    Transform tf;
    Rigidbody rb;
    Camera MainCamera;


    // Start is called before the first frame update
    void Start()
    {
        tf = transform;
        rb = GetComponent<Rigidbody>();
        MainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void Move()
    {
        rb.velocity = MainCamera.transform.forward * m_speed;

    }
}
