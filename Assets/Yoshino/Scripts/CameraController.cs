using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraController : MonoBehaviour
{
    CinemachineFreeLook cinemachine;
    Vector2 input;
    [SerializeField] float sensity;
    // Start is called before the first frame update
    void Start()
    {
        cinemachine = GetComponent<CinemachineFreeLook>();
    }

    // Update is called once per frame
    void Update()
    {
        AxisState Axis;
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            input.x += sensity;

        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            input.x -= sensity;
        }

    }
}
