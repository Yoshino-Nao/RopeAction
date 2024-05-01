using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class PlayerRayCast : MonoBehaviour
{
    //���[�v��������I�u�W�F�N�g�܂ł̈�ԒZ������
    private float MinLength=999;
    //���[�v��������I�u�W�F�N�g�̌��m�t���O
    private bool Checkflag = false;
    private GameObject obj;
    void Update()
    {
        if(Explosion().name == "Enemy")
        {
            Debug.Log("���[�v��������ꏊ����܂��B");
            
        }
        else
        {
            Debug.Log("���[�v��������ꏊ����܂���B");
        }
        
    }

    //�͈͓������ԋ߂��I�u�W�F�N�g����������֐�
    public GameObject Explosion()
    {
        var hits = Physics.SphereCastAll(
            transform.position,     //���S
            5.0f,                   //���a
            Vector3.forward).Select(h => h.transform.gameObject).ToList();    //����

        //Debug.Log($"���o���ꂽ�R���C�_�[�̐�{hits.Length}");

        foreach (var hit in hits)
        {
            //���C�ɐڐG�����I�u�W�F�N�g�̖��O��Enemy�̎�
            if (hit.name == "Enemy")
            {
                //�G�l�~�[���m�t���O
                Checkflag = true;
                //���������߂�
                float Length = Vector3.Distance(transform.position, hit.transform.position);
                //�������Z���Ȃ�
                if (MinLength > Length)
                {
                    //�ŒZ�������X�V
                    MinLength = Length;
                    //�I�u�W�F�ɃG�l�~�[��Ԃ�
                    obj = hit;
                }
            }
            //�G�l�~�[����x�����m���Ȃ����
            else if(!Checkflag)
            {
                //�G�l�~�[�ȊO�̃I�u�W�F�N�g��Ԃ�
                obj = hit;
            }
        }
        //�������Z�b�g
        MinLength = 999;
        //�t���O���Z�b�g
        Checkflag = false;
        
        return obj;
        
    }
}
