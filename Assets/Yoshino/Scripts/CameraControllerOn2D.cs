using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float m_camMoveSpeed = 1;
    [SerializeField] private float m_camRotateAccelTime = 0.5f;
    [SerializeField] private float m_moveToDefSpeed = 0.25f;
    [SerializeField] private float m_maxLength = 3;
    [SerializeField] private float m_maxAngle = 30f;
    //カメラの角度のみを動かす
    [SerializeField] bool m_isOnlyRotation = false;
    //操作していない場合、カメラの位置をデフォルトに戻そうとする
    [SerializeField] bool m_isMoveDefault = true;

    GameInput m_inputs;

    private Vector2 m_cameraInputValue;
    void OnCamera(InputAction.CallbackContext callbackContext)
    {
        m_cameraInputValue = callbackContext.ReadValue<Vector2>();
    }



    void Start()
    {
        m_virtualCamera = GetComponent<CinemachineVirtualCamera>();
        m_transposer = m_virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        m_pov = m_virtualCamera.GetCinemachineComponent<CinemachinePOV>();
        //初期値を設定する
        m_transposer.m_FollowOffset = m_defOffset;
        m_pov.m_HorizontalAxis.m_MaxValue = m_pov.m_VerticalAxis.m_MaxValue = m_maxAngle;
        m_pov.m_HorizontalAxis.m_MinValue = m_pov.m_VerticalAxis.m_MinValue = -m_maxAngle;
        m_pov.m_VerticalAxis.m_AccelTime = m_pov.m_HorizontalAxis.m_AccelTime = m_camMoveSpeed * 10;

        m_pov.m_HorizontalAxis.m_InputAxisName = "";
        m_pov.m_VerticalAxis.m_InputAxisName = "";

        m_pov.m_HorizontalAxis.m_AccelTime = m_pov.m_VerticalAxis.m_AccelTime = m_camRotateAccelTime;

        //入力
        m_inputs = new GameInput();

        m_inputs.Player.Camera.performed += OnCamera;
        m_inputs.Player.Camera.canceled += OnCamera;

        m_inputs.Enable();
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
        h = m_cameraInputValue.x;
        v = m_cameraInputValue.y;

        Vector3 Value = new Vector3(h, v, 0);

        //DebugPrint.Print(string.Format("nullCheck{0}", MPFT_NTD_MMControlSystem.ms_instance != null));


        //カメラを動かす処理
        if (m_isOnlyRotation)
        {
            m_pov.m_HorizontalAxis.Value += h * m_camRotateAccelTime;
            m_pov.m_VerticalAxis.Value -= v * m_camRotateAccelTime;

        }
        else
        {
            m_transposer.m_FollowOffset += Value * m_camMoveSpeed;
            m_pov.m_HorizontalAxis.m_MaxSpeed = 0;
            m_pov.m_VerticalAxis.m_MaxSpeed = 0;

        }

        //入力がない場合は初期位置に補間する
        if (m_isMoveDefault && Value.magnitude <= 0.1)
        {
            m_transposer.m_FollowOffset = Vector3.MoveTowards(m_transposer.m_FollowOffset, m_defOffset, m_moveToDefSpeed);
            m_pov.m_HorizontalAxis.Value = Mathf.MoveTowards(m_pov.m_HorizontalAxis.Value, 0f, m_moveToDefSpeed * 10);
            m_pov.m_VerticalAxis.Value = Mathf.MoveTowards(m_pov.m_VerticalAxis.Value, 0f, m_moveToDefSpeed * 10);
        }

    }
    private void LateUpdate()
    {
        //限界の距離以上は動かないようにする
        {
            Vector2 pos = new Vector2(m_transposer.m_FollowOffset.x, m_transposer.m_FollowOffset.y);
            //長さをクランプする
            Vector2 ClampVec = Vector2.ClampMagnitude(
                pos - new Vector2(m_defOffset.x, m_defOffset.y), m_maxLength);
            m_transposer.m_FollowOffset = new Vector3(ClampVec.x, ClampVec.y + m_defOffset.y, m_defOffset.z);
        }
        {
            Vector2 ClampVec = Vector2.ClampMagnitude(new Vector2(
                m_pov.m_HorizontalAxis.Value, m_pov.m_VerticalAxis.Value
                ), m_maxAngle);
            m_pov.m_HorizontalAxis.Value = ClampVec.x;
            m_pov.m_VerticalAxis.Value = ClampVec.y;

        }
    }
}
