using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //帯をつけられるオブジェクト
    public Transform m_attachTf;
    public GameObject m_Canvas;
    public RectTransform Image;
    Vector3 view;
    void Start()
    {
        
    }
    void Update()
    {
        //ワールド座標からビューポート座標に変換
        view = Camera.main.WorldToViewportPoint(m_attachTf.position);
        //ビューポート座標からスクリーン座標に変換
        Image.position = Camera.main.ViewportToScreenPoint(view);
        Debug.Log(Image.position);

        if(Image.position.z<0)
        {
            m_Canvas.SetActive(false);
        }
        else
        {
            m_Canvas.SetActive(true);
        }

    }
}
