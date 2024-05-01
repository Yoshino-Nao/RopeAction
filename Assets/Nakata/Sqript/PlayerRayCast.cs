using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

public class PlayerRayCast : MonoBehaviour
{
    //ロープをつけられるオブジェクトまでの一番短い距離
    private float MinLength=999;
    //ロープをつけられるオブジェクトの検知フラグ
    private bool Checkflag = false;
    private GameObject obj;
    void Update()
    {
        if(Explosion().name == "Enemy")
        {
            Debug.Log("ロープをつけられる場所あります。");
            
        }
        else
        {
            Debug.Log("ロープをつけられる場所ありません。");
        }
        
    }

    //範囲内から一番近いオブジェクトを検索する関数
    public GameObject Explosion()
    {
        var hits = Physics.SphereCastAll(
            transform.position,     //中心
            5.0f,                   //半径
            Vector3.forward).Select(h => h.transform.gameObject).ToList();    //方向

        //Debug.Log($"検出されたコライダーの数{hits.Length}");

        foreach (var hit in hits)
        {
            //レイに接触したオブジェクトの名前がEnemyの時
            if (hit.name == "Enemy")
            {
                //エネミー検知フラグ
                Checkflag = true;
                //距離を求める
                float Length = Vector3.Distance(transform.position, hit.transform.position);
                //距離が短いなら
                if (MinLength > Length)
                {
                    //最短距離を更新
                    MinLength = Length;
                    //オブジェにエネミーを返す
                    obj = hit;
                }
            }
            //エネミーを一度も検知しなければ
            else if(!Checkflag)
            {
                //エネミー以外のオブジェクトを返す
                obj = hit;
            }
        }
        //距離リセット
        MinLength = 999;
        //フラグリセット
        Checkflag = false;
        
        return obj;
        
    }
}
