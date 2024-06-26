using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using VHierarchy.Libs;

public class ReStart : MonoBehaviour
{
    //�������W
    [SerializeField] private Vector3 SRPosition;
    [SerializeField] private GameObject player;
    void Start()
    {
        //�v���C���[�̏������W�擾
        SRPosition = player.transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //�^�O����
        if (collision.gameObject.tag == "Player")
        {
            //SceneManager.LoadScene(m_sceneName);
            player.transform.position = SRPosition;
        }else{

            Destroy(collision.gameObject);
            
        }
        Debug.Log("��������");
    }
    private void OnCollisionStay(Collision collision)
    {
        //�^�O����
        if (collision.gameObject.tag == "Player")
        {
            //SceneManager.LoadScene(m_sceneName);
            player.transform.position = SRPosition;
        }
        else
        {

            Destroy(collision.gameObject);

        }
        Debug.Log("��������");
    }
}
