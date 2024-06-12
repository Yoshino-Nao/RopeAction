using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
    //初期座標
    public Vector3 SRPosition;
    public GameObject player;
    void Start()
    {
        //プレイヤーの初期座標取得
        SRPosition = player.transform.position;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag== "Player")
        {
            //SceneManager.LoadScene(m_sceneName);
            player.transform.position = SRPosition;
        }
    }
}
