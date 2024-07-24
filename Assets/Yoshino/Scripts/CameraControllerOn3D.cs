using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
public class CameraControllerOn3D : MonoBehaviour
{
    [SerializeField] private CinemachineFreeLook m_cinemachineFreeLook;
    [SerializeField] private float m_sensiX;
    [SerializeField] private float m_sensiY;

    private GameInput m_inputActions;
    private Vector2 m_inputVec;
    // Start is called before the first frame update
    void Start()
    {
        m_inputActions = new GameInput();

        m_inputActions.Player.Camera.performed += OnCamera;
        m_inputActions.Player.Camera.canceled += OnCamera;

        m_inputActions.Enable();
    }

    // Update is called once per frame
    void LateUpdate()
    {

        DebugPrint.Print(string.Format("CameraInput{0}", m_inputVec));
        m_cinemachineFreeLook.m_XAxis.Value += m_inputVec.x * m_sensiX * Time.deltaTime;
        m_cinemachineFreeLook.m_YAxis.Value += m_inputVec.y * m_sensiY * Time.deltaTime;

    }
    void OnCamera(InputAction.CallbackContext context)
    {
        m_inputVec = context.ReadValue<Vector2>();
    }
}
