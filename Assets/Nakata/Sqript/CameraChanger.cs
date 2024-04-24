using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraChanger : MonoBehaviour
{
    [SerializeField,Header("2D�J����")]
    private GameObject Camera2D;
    [SerializeField,Header("3D�J����")]
    private GameObject Camera3D;
    private bool CameraChenge = true;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("�J������ύX���܂��B");
        if(CameraChenge)
        {
            Camera3D.SetActive(true);
            Camera2D.SetActive(false);
            CameraChenge = false;
        }else if(!CameraChenge)
        {
            Camera3D.SetActive(false);
            Camera2D.SetActive(true);
            CameraChenge = true;
        }
    }
}
