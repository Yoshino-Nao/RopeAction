using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //帯をつけられるオブジェクト
    private Transform m_attachTf;
    public Transform SetAttachmentTarget
    {
        set { m_attachTf = value; }
    }
    private RectTransform rectTf;
    private Image image;
    private Vector3 view;
    void Awake()
    {
        rectTf = GetComponentInParent<RectTransform>();
        image = GetComponentInParent<Image>();
        image.enabled = true;
    }
    void LateUpdate()
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
