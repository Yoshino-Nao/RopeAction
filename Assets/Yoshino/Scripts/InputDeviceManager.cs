using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputDeviceManager : MonoBehaviour
{
    //シングルトン
    public static InputDeviceManager Instance { get; private set; }
    /// <summary>
    /// 入力デバイスの種別
    /// </summary>
    public enum eDeviceType
    {
        KeyBoard,
        DualSense,
        NintendoSwitch
    }
    //直近に操作された入力デバイスタイプ
    public eDeviceType m_CurrentDeviceType { get; private set; } = eDeviceType.KeyBoard;

    //各デバイスの全てのキーを1つにバインドしたInputAction(キー種別検知用)
    private InputAction m_keyboardAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<KeyBoard>/AnyKey", interactions: "Press");
    private InputAction m_mouseAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<Mouse>/*", interactions: "Press");
    private InputAction m_dualSenseAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<DualSenseGamepadHID>/*", interactions: "Press");
    private InputAction m_switchAnyKey = new InputAction(
        type: InputActionType.PassThrough, binding: "<Gamepad>/*", interactions: "Press");


    //入力デバイスタイプ変更イベント
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
        // 初回、またシーンが読み込まれたとき、必ず入力デバイスの種別検知を行ってコールバック発火
        StartCoroutine(InitializeDetection());
        SceneManager.sceneLoaded += OnSceneLoad;
    }
    // Update is called once per frame
    void Update()
    {
        // 検知の更新処理
        UpdateDeviceTypesDetection();

        // デバイス一覧を取得
        //foreach (var device in InputSystem.devices)
        //{
        //    // デバイスのパスをログ出力
        //    Debug.LogError(device.path);
        //}
    }
    private void OnSceneLoad(Scene scene,LoadSceneMode mode)
    {
        StartCoroutine(InitializeDetection());
    }
    /// <summary>
    /// 入力デバイスの種別検知を初期化する
    /// </summary>
    /// <returns></returns>
    IEnumerator InitializeDetection()
    {
        UpdateDeviceTypesDetection();

        yield return null;

        OnChangeDeviceType.Invoke();
    }

    /// <summary>
    /// 入力デバイスの種別検知を更新する
    /// </summary>
    public void UpdateDeviceTypesDetection()
    {
        eDeviceType beforeDeviceType = m_CurrentDeviceType;

        if (m_dualSenseAnyKey.triggered)
        {
            m_CurrentDeviceType = eDeviceType.DualSense;
        }
        if (m_switchAnyKey.triggered)
        {
            m_CurrentDeviceType = eDeviceType.NintendoSwitch;
        }
        if (m_keyboardAnyKey.triggered || m_mouseAnyKey.triggered)
        {
            m_CurrentDeviceType = eDeviceType.KeyBoard;
        }
        // 操作デバイスが切り替わったとき、イベント発火
        if (beforeDeviceType != m_CurrentDeviceType)
        {
            OnChangeDeviceType.Invoke();
        }
    }
}
