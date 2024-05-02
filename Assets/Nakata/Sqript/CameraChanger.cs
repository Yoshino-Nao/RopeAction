using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VInspector;

public class CameraChanger : MonoBehaviour
{
    [SerializeField, Header("2Dカメラ")]
    private CinemachineVirtualCamera virtualCamera2D = null;

    [SerializeField, Header("3Dカメラ")]
    private CinemachineFreeLook virtualCamera3D = null;
    [Button]
    void CameraChangeButton()
    {
        CameraChange();
    }
    private bool m_toggle = false;
    public bool m_is3DCamera = true;
    void Start()
    {
        //
        m_toggle = m_is3DCamera;
        if (m_is3DCamera)
        {
            Set3DCamera();
        }
        else if (!m_is3DCamera)
        {
            Set2DCamera();
        }
    }

    public void CameraChange()
    {
        Debug.Log("カメラを変更します。");
        if (m_toggle)
        {
            Set2DCamera();
            m_toggle = false;
        }
        else if (!m_toggle)
        {
            Set3DCamera();
            m_toggle = true;
        }
    }

    private void Set3DCamera()
    {
        virtualCamera2D.Priority = 0;
        virtualCamera3D.Priority = 1;
        m_is3DCamera = true;
    }

    private void Set2DCamera()
    {
        virtualCamera2D.Priority = 1;
        virtualCamera3D.Priority = 0;
        m_is3DCamera = false;
    }
}
