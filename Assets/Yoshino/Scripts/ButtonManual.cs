using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static InputDeviceManager;

public class ButtonManual : MonoBehaviour
{
    private const string m_path = "Assets/Resources/ScriptableObjects/Manual Image Data.asset";
    private const string m_resourcesPath = "ScriptableObjects/Manual Image Data";
    public Image m_moveImage;
    public Image m_jumpImage;
    public Image m_ropeAttachImage;
    public Image m_ropeControlImage;

    private void LoadImageData()
    {
#if UNITY_EDITOR
        m_imageData = AssetDatabase.LoadAssetAtPath<ManualSpritesData>(m_path);
#else
        m_imageData = Resources.Load<ManualImageData>(m_resourcesPath);
#endif

    }
    private ManualSpritesData m_imageData;


    void ImageChange()
    {
        if (m_imageData == null)
        {
            LoadImageData();
        }


        switch (Instance.m_CurrentDeviceType)
        {
            case eDeviceType.KeyBoard:
                m_moveImage.sprite = m_imageData.m_keyboardAndMouse.Move;
                m_jumpImage.sprite = m_imageData.m_keyboardAndMouse.Jump;
                m_ropeAttachImage.sprite = m_imageData.m_keyboardAndMouse.RopeAttach;
                m_ropeControlImage.sprite = m_imageData.m_keyboardAndMouse.RopeControl;
                break;
            case eDeviceType.DualSense:
                m_moveImage.sprite = m_imageData.m_dualSense.Move;
                m_jumpImage.sprite = m_imageData.m_dualSense.Jump;
                m_ropeAttachImage.sprite = m_imageData.m_dualSense.RopeAttach;
                m_ropeControlImage.sprite = m_imageData.m_dualSense.RopeControl;
                break;
        }
    }
    void Start()
    {
        LoadImageData();
        Instance.OnChangeDeviceType.AddListener(ImageChange);
    }

}
