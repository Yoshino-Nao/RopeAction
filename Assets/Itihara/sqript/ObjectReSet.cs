using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using VInspector.Libs;

public class ObjectReSet : MonoBehaviour
{
    public Transform m_Master;
    //List<Transform> m_G;
    Dictionary<Transform, Vector3> m_V = new Dictionary<Transform, Vector3>();
    //List<Vector3> m_V;
    //Vector3 pos;
    Player player;
    void Start()
    {
        player = FindObjectOfType<Player>();
        //���̍��W�ɖ߂����߁A���݂̍��W�����
        foreach (Transform dummy in m_Master)
        {
            //m_G.Add(dummy);
            //pos = dummy.position;
            m_V.Add(dummy, dummy.position);
            //m_V.Add(pos);
        }
    }
    //�G�ꂽ�Ƃ����W�����Ƃɖ߂��āAIskinematic��true�ɂ���
    private void OnCollisionEnter(Collision collision)
    {
        Transform tf = collision.transform;
        if (m_V.ContainsKey(tf))
        {
            tf.position = m_V[tf];
            tf.GetComponent<Rigidbody>().isKinematic = true;
           player.Detach();
        }
    }
}
