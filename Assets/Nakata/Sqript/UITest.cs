using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //帯をつけられるオブジェクト
    public GameObject obj;
    public RectTransform Image;
    Vector3 view;
    void Start()
    {
        
    }
    void Update()
    {
        //ワールド座標からビューポート座標に変換
        view = Camera.main.WorldToViewportPoint(obj.transform.position);
        //ビューポート座標からスクリーン座標に変換
        Image.position = Camera.main.ViewportToScreenPoint(view);

    }
}
