using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
    //初期座標
     private Vector3 SRPosition;
    [SerializeField] private Transform Player;
    Player player;
    void Start()
    {
        player = FindObjectOfType<Player>();
        //プレイヤーの初期座標取得
        SRPosition = Player.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //タグ検索
        if (collision.collider.GetComponent<Player>()!=null)
        {

            //SceneManager.LoadScene(m_sceneName);
            Player.position = SRPosition;
            player.Detach();
        }else if(collision.collider.GetComponent<fallObject>()){

            Destroy(collision.gameObject);
            
        }
        //Debug.Log("当たった");
    }
}
