using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class CameraChanger : MonoBehaviour
{
    [SerializeField,Header("2D�J����")]
    private GameObject Camera2D;
    [SerializeField,Header("3D�J����")]
    private GameObject Camera3D;
    [Button]
   void CameraChangeButton()
    {
        CameraChange();
    }
    public bool is2D = true;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void CameraChange()
    {
        Debug.Log("�J������ύX���܂��B");
        if(is2D)
        {
            Camera3D.SetActive(true);
            Camera2D.SetActive(false);
            is2D = false;
        }else if(!is2D)
        {
            Camera3D.SetActive(false);
            Camera2D.SetActive(true);
            is2D = true;
        }
    }
}
