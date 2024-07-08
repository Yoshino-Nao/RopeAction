using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
    //初期座標
    [SerializeField] private Vector3 SRPosition;
    [SerializeField] private GameObject player;
    void Start()
    {
        //プレイヤーの初期座標取得
        SRPosition = player.transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        //タグ検索
        if (collision.gameObject.tag == "Player")
        {
             
            //SceneManager.LoadScene(m_sceneName);
            player.transform.position = SRPosition;
        }else{

            Destroy(collision.gameObject);
            
        }
        //Debug.Log("当たった");
    }
}
