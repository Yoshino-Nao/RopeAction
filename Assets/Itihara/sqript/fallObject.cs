using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Obi;

public class fallObject : MonoBehaviour
{
    public ObiRope rope;
    public float tensionThreshold = 10.0f; // ��������Ɣ��肷��͂�臒l
    public Rigidbody targetRigidbody; // ����Ώۂ�Rigidbody
    private float time = 0.0f;
    private bool check;
    public float WaitTime = 3.0f;

   public void  IsRopeUnderTension()
    {
        time += UnityEngine.Time.deltaTime;
        var solver = rope.solver;
        float tension = rope.CalculateLength() / rope.restLength - 1;
        //DebugPrint.Print(string.Format("{0}", tension));
        if (tension > tensionThreshold)
        {
            time += UnityEngine.Time.deltaTime;
            DebugPrint.Print(string.Format("{0}", time));
            if (time >= WaitTime)
                check = true;
            if (check)
                body.isKinematic = false;
        }

       
    }
    //[SerializeField]
    //private struct NoiseParam
    //{

    //    //�U��
    //    public float amplitude;

    //    //�U���̑���
    //    public float speed;

    //    //�p�[�����m�C�Y�̃I�t�Z�b�g
    //    [NonSerialized] public float offset;

    //    //�����̃I�t�Z�b�g�l���w�肷��
    //    public void SetRandomOffset()
    //    {
    //        offset = UnityEngine.Random.Range(0f, 256f);
    //    }

    //    //�w�莞���̃p�[�����m�C�Y�l���擾
    //}
    Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    //�Փ˂������Aobject�j��
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "floor")
    //    {
    //        if(!body.isKinematic)
    //        body.isKinematic = true;
    //    }
    //}
    
      
 
}