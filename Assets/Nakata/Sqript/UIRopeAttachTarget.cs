using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static InputDeviceManager;
public class UIRopeAttachTarget : MonoBehaviour
{
    //�т�������I�u�W�F�N�g
    private Transform m_attachTf;
    public Transform SetAttachmentTarget
    {
        set { m_attachTf = value; }
    }
    private RectTransform m_rectTf;
    private Image m_targetImage;
    [SerializeField] private Image m_circleImage;
    [SerializeField] private float m_rotateSpeed = 50;
    private ButtonManual m_buttonManual;
    private Vector3 m_view;

    void Start()
    {
        m_rectTf = GetComponentInParent<RectTransform>();
        m_targetImage = GetComponent<Image>();
        m_targetImage.enabled = true;
        m_buttonManual = FindObjectOfType<ButtonManual>();
        Instance.OnChangeDeviceType.AddListener(SpriteChange);
    }
    void LateUpdate()
    {
        if (m_attachTf != null)
        {
            //���[���h���W����r���[�|�[�g���W�ɕϊ�
            m_view = Camera.main.WorldToViewportPoint(m_attachTf.position);
        }



        m_targetImage.enabled = m_circleImage.enabled = m_view.z > 0 && m_attachTf;


        //�r���[�|�[�g���W����X�N���[�����W�ɕϊ�
        m_rectTf.position = Camera.main.ViewportToScreenPoint(m_view);
        m_circleImage.transform.rotation = m_circleImage.transform.rotation * Quaternion.AngleAxis(m_rotateSpeed * Time.deltaTime, Vector3.forward);

    }
    private void SpriteChange()
    {
        m_targetImage.sprite = m_buttonManual.m_ropeAttachImage.sprite;
    }
}
