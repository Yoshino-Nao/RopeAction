using UnityEngine;
using Cinemachine;

public class CameraController2D : MonoBehaviour
{
    private CinemachineVirtualCamera m_virtualCamera;
    private CinemachineTransposer m_transposer;
    private CinemachinePOV m_pov;
    enum eInputMode
    {
        Mouse,
        KeyArrow
    }
    [SerializeField] private eInputMode m_mode = eInputMode.Mouse;
    [SerializeField] private Vector3 m_defOffset = new Vector3(0, 1.5f, -7);
    [SerializeField] private float m_comMoveSpeed = 1;
    [SerializeField] private float m_camRotateAccelTime = 0.5f;
    [SerializeField] private float m_moveToDefSpeed = 0.25f;
    [SerializeField] private float m_maxLength = 3;
    [SerializeField] private float m_mexAngle = 30f;
    //カメラの角度のみを動かす
    [SerializeField] bool m_isOnlyRotation = false;
    //操作していない場合、カメラの位置をデフォルトに戻そうとする
    [SerializeField] bool m_isMoveDefault = true;
    private float m_povSpeedDef;
    // Start is called before the first frame update
    void Start()
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
        m_transposer = m_virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_pov = m_virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        //初期値を設定する
        m_transposer.m_FollowOffset = m_defOffset;
        m_pov.m_HorizontalAxis.m_MaxValue = m_pov.m_VerticalAxis.m_MaxValue = m_mexAngle;
        m_pov.m_HorizontalAxis.m_MinValue = m_pov.m_VerticalAxis.m_MinValue = -m_mexAngle;
        m_pov.m_VerticalAxis.m_AccelTime = m_pov.m_HorizontalAxis.m_AccelTime = m_comMoveSpeed * 10;

        m_povSpeedDef = m_pov.m_HorizontalAxis.m_MaxSpeed;
        m_pov.m_HorizontalAxis.m_AccelTime = m_pov.m_VerticalAxis.m_AccelTime = m_camRotateAccelTime;
    }

    // Update is called once per frame
    void Update()
    {
        float h = 0;
        float v = 0;
        if (MPFT_NTD_MMControlSystem.ms_instance != null)
        {
            h = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.R_Analog_X;
            v = MPFT_NTD_MMControlSystem.ms_instance.SGGamePad.R_Analog_Y;
        }
        else if (m_mode == eInputMode.Mouse)
        {
            h = Input.GetAxis("Mouse X");
            v = Input.GetAxis("Mouse Y");
            //m_pov.m_HorizontalAxis.m_InputAxisName = "Mouse X";
            //m_pov.m_VerticalAxis.m_InputAxisName = "Mouse Y";

        }
        else
        {
            h = Input.GetAxis("HorizontalArrow");
            v = Input.GetAxis("VerticalArrow");
            //m_pov.m_HorizontalAxis.m_InputAxisName = "HorizontalArrow";
            //m_pov.m_VerticalAxis.m_InputAxisName = "VerticalArrow";
        }

        Vector3 Value = new Vector3(h, v, 0);
        DebugPrint.Print(string.Format("nullCheck{0}", MPFT_NTD_MMControlSystem.ms_instance != null));
        DebugPrint.Print(string.Format("CameraInput{0}", Value));
        ////入力がない場合は初期位置に補間する
        if (Value.magnitude > 0)
        {
            if (m_isOnlyRotation)
            {
                m_pov.m_HorizontalAxis.m_InputAxisValue = h;
                m_pov.m_VerticalAxis.m_InputAxisValue = v;

                //m_pov.m_HorizontalAxis.m_MaxSpeed = m_povSpeedDef;
                //m_pov.m_VerticalAxis.m_MaxSpeed = m_povSpeedDef;
            }
            else
            {
                m_transposer.m_FollowOffset += Value * m_comMoveSpeed;
                m_pov.m_HorizontalAxis.m_MaxSpeed = 0;
                m_pov.m_VerticalAxis.m_MaxSpeed = 0;

            }
            //m_pov.m_HorizontalAxis.Value += Value.x * (m_povMaxValueX * m_comMoveSpeed);
            //m_pov.m_VerticalAxis.Value += Value.y * m_comMoveSpeed;
        }
        else if (m_isMoveDefault)
        {
            m_transposer.m_FollowOffset = Vector3.MoveTowards(m_transposer.m_FollowOffset, m_defOffset, m_moveToDefSpeed);
            m_pov.m_HorizontalAxis.Value = Mathf.MoveTowards(m_pov.m_HorizontalAxis.Value, 0f, m_moveToDefSpeed * 10);
            m_pov.m_VerticalAxis.Value = Mathf.MoveTowards(m_pov.m_VerticalAxis.Value, 0f, m_moveToDefSpeed * 10);
        }
        //限界の距離以上は動かないようにする
        {
            Vector2 pos = new Vector2(m_transposer.m_FollowOffset.x, m_transposer.m_FollowOffset.y);
            //Vector2で長さをクランプする
            Vector2 ClampVec = Vector2.ClampMagnitude(
                pos - new Vector2(m_defOffset.x, m_defOffset.y), m_maxLength);
            m_transposer.m_FollowOffset = new Vector3(ClampVec.x, ClampVec.y + m_defOffset.y, m_defOffset.z);
        }
    }
}
