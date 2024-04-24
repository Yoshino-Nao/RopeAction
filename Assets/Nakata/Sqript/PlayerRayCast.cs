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
            transform.position,     //中心
            5.0f,                   //半径
            Vector3.forward);       //方向

        Debug.Log($"検出されたコライダーの数{hits.Length}");

        foreach (var hit in hits)
        {
            //距離を求める
            float Length = Vector3.Distance(transform.position, hit.transform.position);
            //距離が短いなら
            if(MinLength>Length)
            {
                //最短距離を更新
                MinLength = Length;
                //一番短い距離のオブジェクトに更新する
                obj = hit;
            }
        }
        //距離リセット
        Debug.Log($"一番短い距離のオブジェクト{obj.collider.name}{MinLength}");
        MinLength = 10;
        return obj;
    }
}
