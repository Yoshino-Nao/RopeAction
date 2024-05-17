using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //帯をつけられるオブジェクト
    public Transform m_attachTf;
    public GameObject m_Canvas;
    public RectTransform rectTf;
    public Image image;
    Vector3 view;
    void Start()
    {

    }
    void Update()
    {
        if (m_attachTf != null)
        {
            //ワールド座標からビューポート座標に変換
            view = Camera.main.WorldToViewportPoint(m_attachTf.position);
        }


        image.enabled = view.z > 0 && m_attachTf;

        //ビューポート座標からスクリーン座標に変換
        rectTf.position = Camera.main.ViewportToScreenPoint(view);
        //Debug.Log(rectTf.position);

        //if(Image.position.z<0)
        //{
        //    m_Canvas.SetActive(false);
        //}
        //else
        //{
        //    m_Canvas.SetActive(true);
        //}

    }
}
