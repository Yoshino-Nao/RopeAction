using UnityEngine;
using Cinemachine;

public class CameraController2D : MonoBehaviour
{
    private CinemachineVirtualCamera m_virtualCamera;
    private CinemachineTransposer m_transposer;
    private CinemachineComposer m_composer;
    enum eInputMode
    {
        Mouse,
        KeyArrow
    }
    [SerializeField] private eInputMode m_mode = eInputMode.Mouse;
    [SerializeField] private Vector3 m_defOffset;
    [SerializeField] private float m_comMoveSpeed = 3;
    [SerializeField] private float m_moveToDefSpeed;
    [SerializeField] private float m_maxLength = 3;
    [SerializeField] bool test;
    // Start is called before the first frame update
    void Start()
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
        m_transposer = m_virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_composer = m_virtualCamera.GetCinemachineComponent<CinemachineComposer>();
        //初期値
        m_transposer.m_FollowOffset = m_defOffset;
        m_composer.m_TrackedObjectOffset = new Vector3(0, m_defOffset.y, 0);
    }

    // Update is called once per frame
    void Update()
    {
        float h;
        float v;
        if (m_mode == eInputMode.Mouse)
        {
            h = Input.GetAxis("Mouse X");
            v = Input.GetAxis("Mouse Y");
        }
        else
        {
            h = Input.GetAxis("HorizontalArrow");
            v = Input.GetAxis("VerticalArrow");
        }
        Vector3 Value = new Vector3(h, v, 0);

        ////入力がない場合は初期位置に補間する
        if (Value.magnitude > 0)
        {
            if (test)
            {
                m_transposer.m_FollowOffset += Value * m_comMoveSpeed;
            }
            m_composer.m_TrackedObjectOffset += Value * m_comMoveSpeed;
        }
        else
        {
            m_transposer.m_FollowOffset = Vector3.MoveTowards(m_transposer.m_FollowOffset, m_defOffset, m_moveToDefSpeed);
            m_composer.m_TrackedObjectOffset = Vector3.MoveTowards(m_composer.m_TrackedObjectOffset, new Vector3(0, m_defOffset.y, 0), m_moveToDefSpeed);
        }
        //限界の距離以上は動かないようにする
        {
            Vector2 pos = new Vector2(m_transposer.m_FollowOffset.x, m_transposer.m_FollowOffset.y);
            //Vector2で長さをクランプする
            Vector2 ClampVec = Vector2.ClampMagnitude(
                pos - new Vector2(m_defOffset.x, m_defOffset.y), m_maxLength);
            m_transposer.m_FollowOffset = new Vector3(ClampVec.x, ClampVec.y + m_defOffset.y, m_defOffset.z);
        }
        {
            Vector2 pos = new Vector2(m_composer.m_TrackedObjectOffset.x, m_composer.m_TrackedObjectOffset.y);
            Vector2 ClampVec = Vector2.ClampMagnitude(
                 pos - new Vector2(0, m_defOffset.y), m_maxLength);
            m_composer.m_TrackedObjectOffset = new Vector3(ClampVec.x, ClampVec.y + m_defOffset.y, 0);
        }
    }
}
