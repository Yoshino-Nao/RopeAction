using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : MonoBehaviour
{
    //�����^�[�Q�b�g
    private int point = 0;
    [SerializeField, Header("�ړ����x")]
    private float moveSpeed;
    [SerializeField, Header("�ړ����[�g")]
    private Transform []target;

    void Update()
    {
        //�ړI�n�܂ňړ����鏈��
        transform.position = Vector3.MoveTowards(transform.position, target[point].position, moveSpeed * Time.deltaTime);
        //�ړI�n�ɓ���������
        if(transform.position == target[point].position)
        {
            //�^�[�Q�b�g�����̒n�_�ɕύX
            point++;
        }
        //�ŏI�ړI�n�ɓ��B������ړ������[�v����
        if(point == target.Length)
        {
            point = 0;
        }
        
    }
}
