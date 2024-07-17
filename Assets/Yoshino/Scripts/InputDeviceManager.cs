using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputDeviceManager : MonoBehaviour
{
    //�V���O���g��
    public static InputDeviceManager Instance { get; private set; }
    /// <summary>
    /// ���̓f�o�C�X�̎��
    /// </summary>
    public enum eDeviceType
    {
        KeyBoard,
        DualSense,

    }
    //���߂ɑ��삳�ꂽ���̓f�o�C�X�^�C�v
    public eDeviceType m_CurrentDeviceType { get; private set; } = eDeviceType.KeyBoard;

    //�e�f�o�C�X�̑S�ẴL�[��1�Ƀo�C���h����InputAction(�L�[��ʌ��m�p)
    private InputAction m_keyboardAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<KeyBoard>/AnyKey", interactions: "Press");
    private InputAction m_mouseAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<Mouse>/*", interactions: "Press");
    private InputAction m_dualSenseAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<DualSenseGamepadHID>/*", interactions: "Press");

    //���̓f�o�C�X�^�C�v�ύX�C�x���g
    public UnityEvent OnChangeDeviceType { get; private set; } = new();

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        m_keyboardAnyKey.Enable();
        m_mouseAnyKey.Enable();
        m_dualSenseAnyKey.Enable();
    }
    private void Start()
    {
        // ����̂݁A�K�����̓f�o�C�X�̎�ʌ��m���s���ăR�[���o�b�N����
        StartCoroutine(InitializeDetection());
    }
    // Update is called once per frame
    void Update()
    {
        // ���m�̍X�V����
        UpdateDeviceTypesDetection();
    }

    /// <summary>
    /// ���̓f�o�C�X�̎�ʌ��m������������
    /// </summary>
    /// <returns></returns>
    IEnumerator InitializeDetection()
    {
        UpdateDeviceTypesDetection();

        yield return null;

        OnChangeDeviceType.Invoke();
    }

    /// <summary>
    /// ���̓f�o�C�X�̎�ʌ��m���X�V����
    /// </summary>
    public void UpdateDeviceTypesDetection()
    {
        eDeviceType beforeDeviceType = m_CurrentDeviceType;

        if (m_dualSenseAnyKey.triggered)
        {
            m_CurrentDeviceType = eDeviceType.DualSense;
        }

        if (m_keyboardAnyKey.triggered || m_mouseAnyKey.triggered)
        {
            m_CurrentDeviceType = eDeviceType.KeyBoard;
        }
        // ����f�o�C�X���؂�ւ�����Ƃ��A�C�x���g����
        if (beforeDeviceType != m_CurrentDeviceType)
        {
            OnChangeDeviceType.Invoke();
        }
    }
}