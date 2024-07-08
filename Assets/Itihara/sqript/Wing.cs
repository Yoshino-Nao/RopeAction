using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wing : MonoBehaviour
{
    public float MathfTime = 0.3f;
    Vector3 startPos;
    // Start is called before the first frame update
    void Start()
    { 
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float posY = startPos.y + Mathf.Sin(Time.time) * MathfTime;
        transform.position = new Vector3(transform.position.x, posY, transform.position.z);

    }
}
