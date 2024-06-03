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
    [SerializeField, Header("�N�[���^�C��")]
    private float Timer;
    private float nowTimer;

    void FixedUpdate()
    {
        if(Timer > nowTimer)
        {
            nowTimer += Time.deltaTime;
        }
        else
        {
            //�ړI�n�܂ňړ����鏈��
            transform.position = Vector3.MoveTowards(transform.position, target[point].position, moveSpeed * Time.deltaTime);
            //�ړI�n�ɓ���������
            if (transform.position == target[point].position)
            {
                //�^�[�Q�b�g�����̒n�_�ɕύX
                point++;
                nowTimer = 0;
            }
            //�ŏI�ړI�n�ɓ��B������ړ������[�v����
            if (point == target.Length)
            {
                point = 0;
            }
        }
    }
}
