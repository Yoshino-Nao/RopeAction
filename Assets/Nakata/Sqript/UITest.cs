using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //�т�������I�u�W�F�N�g
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
            //���[���h���W����r���[�|�[�g���W�ɕϊ�
            view = Camera.main.WorldToViewportPoint(m_attachTf.position);
        }


        image.enabled = view.z > 0 && m_attachTf;

        //�r���[�|�[�g���W����X�N���[�����W�ɕϊ�
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
