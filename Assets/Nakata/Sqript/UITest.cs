using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //�т�������I�u�W�F�N�g
    [HideInInspector] public Transform m_attachTf;
    private RectTransform rectTf;
    private Image image;
    Vector3 view;
    void Start()
    {
        rectTf = GetComponent<RectTransform>();
        image = GetComponent<Image>();
    }
    void LateUpdate()
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
