using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController2D : MonoBehaviour
{
    private CinemachineVirtualCamera m_virtualCamera;
    private CinemachineTransposer m_transposer;
    private CinemachineComposer m_composer;
    private Vector3 m_offset;
    [SerializeField] private Vector3 m_defOffset;
    [SerializeField] private float m_pow = 3;
    [SerializeField] private float m_maxLength = 3;
    // Start is called before the first frame update
    void Start()
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
        m_transposer = m_virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_composer = m_virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        m_offset = m_defOffset;
        //‰Šú’l
        m_transposer.m_FollowOffset = m_defOffset;
        m_composer.m_TrackedObjectOffset = new Vector3(0, m_defOffset.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");
        Vector3 Value = new Vector3(h, v, 0) * m_pow;
        //DebugPrint.Print(string.Format("{0}", new Vector2(h, v)));

        //m_transposer.m_FollowOffset += Value;
        //if ((m_transposer.m_FollowOffset- m_defOffset).magnitude >= m_maxLength) 
        //{
        //    m_transposer.m_FollowOffset = Vector3.ClampMagnitude(m_transposer.m_FollowOffset, m_maxLength);
        //    m_transposer.m_FollowOffset.z = m_defOffset.z;
        //}
        //DebugPrint.Print(string.Format("{0}{1}", (m_transposer.m_FollowOffset - m_defOffset).magnitude, m_maxLength));
        m_composer.m_TrackedObjectOffset += Value;
        if (Vector3.Distance(m_composer.m_TrackedObjectOffset, new Vector3(0, m_defOffset.y, 0)) >= m_maxLength)
        {
            m_composer.m_TrackedObjectOffset = Vector3.ClampMagnitude(m_composer.m_TrackedObjectOffset, m_maxLength);
        }
    }
}
