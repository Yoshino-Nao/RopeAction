using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawn : MonoBehaviour
{
    [SerializeField, Header("リスポーン地点")]
    private Vector3 spawn;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //プレイヤーの位置をリスポーン地点に移動
            other.transform.position = spawn;
        }
    }
}
