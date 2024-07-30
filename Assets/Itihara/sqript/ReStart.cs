using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
    //�������W
     private Vector3 SRPosition;
    [SerializeField] private Transform Player;
    Player player;
    void Start()
    {
        player = FindObjectOfType<Player>();
        //�v���C���[�̏������W�擾
        SRPosition = Player.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //�^�O����
        if (collision.collider.GetComponent<Player>()!=null)
        {

            //SceneManager.LoadScene(m_sceneName);
            Player.position = SRPosition;
            player.Detach();
        }else if(collision.collider.GetComponent<fallObject>()){

            Destroy(collision.gameObject);
            
        }
        //Debug.Log("��������");
    }
}
