using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class respawn : MonoBehaviour
{
    [SerializeField, Header("���X�|�[���n�_")]
    private Vector3 spawn;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //�v���C���[�̈ʒu�����X�|�[���n�_�Ɉړ�
            other.transform.position = spawn;
        }
    }
}
