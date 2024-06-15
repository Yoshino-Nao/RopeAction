using UnityEngine;

public class Change2DAnd3D : MonoBehaviour
{
    private enum eType
    {
        To3D,
        To2D
    }
    [SerializeField] eType type;

    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            return;
        }

        switch (type)
        {
            case eType.To3D:
                CameraChanger.ms_instance.Set3DCamera();
                break;
            case eType.To2D:
                CameraChanger.ms_instance.Set2DCamera();
                other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, transform.position.z);
                break;
        }

    }
}
