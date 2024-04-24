using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRayCast : MonoBehaviour
{
    private float MinLength=10;
    private RaycastHit obj;
    void Update()
    {
        Explosion();
    }

    private RaycastHit Explosion()
    {
        RaycastHit[] hits = Physics.SphereCastAll(
            transform.position,     //���S
            5.0f,                   //���a
            Vector3.forward);       //����

        Debug.Log($"���o���ꂽ�R���C�_�[�̐�{hits.Length}");

        foreach (var hit in hits)
        {
            //���������߂�
            float Length = Vector3.Distance(transform.position, hit.transform.position);
            //�������Z���Ȃ�
            if(MinLength>Length)
            {
                //�ŒZ�������X�V
                MinLength = Length;
                //��ԒZ�������̃I�u�W�F�N�g�ɍX�V����
                obj = hit;
            }
        }
        //�������Z�b�g
        Debug.Log($"��ԒZ�������̃I�u�W�F�N�g{obj.collider.name}{MinLength}");
        MinLength = 10;
        return obj;
    }
}
