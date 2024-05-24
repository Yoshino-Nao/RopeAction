using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Obi;

public class fallObject : MonoBehaviour
{
    public ObiRope rope;
    public float tensionThreshold = 10.0f; // 引っ張りと判定する力の閾値
    public Rigidbody targetRigidbody; // 操作対象のRigidbody
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

    //    //振動
    //    public float amplitude;

    //    //振動の速さ
    //    public float speed;

    //    //パーリンノイズのオフセット
    //    [NonSerialized] public float offset;

    //    //乱数のオフセット値を指定する
    //    public void SetRandomOffset()
    //    {
    //        offset = UnityEngine.Random.Range(0f, 256f);
    //    }

    //    //指定時刻のパーリンノイズ値を取得
    //}
    Rigidbody body;

    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }
    //衝突した時、object破壊
    //void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.tag == "floor")
    //    {
    //        if(!body.isKinematic)
    //        body.isKinematic = true;
    //    }
    //}
    
      
 
}