using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITest : MonoBehaviour
{
    //�т�������I�u�W�F�N�g
    public GameObject obj;
    public RectTransform Image;
    Vector3 view;
    void Start()
    {
        
    }
    void Update()
    {
        //���[���h���W����r���[�|�[�g���W�ɕϊ�
        view = Camera.main.WorldToViewportPoint(obj.transform.position);
        //�r���[�|�[�g���W����X�N���[�����W�ɕϊ�
        Image.position = Camera.main.ViewportToScreenPoint(view);

    }
}
