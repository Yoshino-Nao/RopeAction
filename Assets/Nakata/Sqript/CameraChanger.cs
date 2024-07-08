using Cinemachine;
using UnityEngine;
using VInspector;

public class CameraChanger : MonoBehaviour
{
    public static CameraChanger ms_instance;
    //先頭に数字使えない
    enum eDefaultMode
    {
        _3D,
        _2D,
    }
    [SerializeField] eDefaultMode m_defaultMode;
    [SerializeField, Header("2Dカメラ")]
    private CinemachineVirtualCamera virtualCamera2D = null;
    Player m_player;
    [SerializeField, Header("3Dカメラ")]
    private CinemachineFreeLook virtualCamera3D = null;
    [Button]
    void CameraChangeButton()
    {
        SwitchingCamera();
    }
    public bool m_is3DCamera = true;
    void Start()
    {
        if (ms_instance == null)
        {
            ms_instance = this;
        }
        else
        {
            //プレイヤーを２人出さないかぎり使わない
            Destroy(gameObject);
        }
        m_player = FindAnyObjectByType<Player>();
        CameraSetUp();
        if (m_defaultMode == eDefaultMode._3D)
        {
            Set3DCamera();
        }
        else
        {
            Set2DCamera();
        }
    }

    public void SwitchingCamera()
    {
        Debug.Log("カメラを変更します。");
        if (m_is3DCamera)
        {
            Set2DCamera();
        }
        else if (!m_is3DCamera)
        {
            Set3DCamera();
        }
    }

    public void Set3DCamera()
    {
        virtualCamera2D.Priority = 0;
        virtualCamera3D.Priority = 1;
        m_is3DCamera = true;
    }

    public void Set2DCamera()
    {
        virtualCamera2D.Priority = 1;
        virtualCamera3D.Priority = 0;
        m_is3DCamera = false;
    }
    private void CameraSetUp()
    {
        virtualCamera2D.Follow = virtualCamera3D.Follow = m_player.transform;
        virtualCamera2D.LookAt = virtualCamera3D.LookAt = m_player.transform;
    }
}
