using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPrint : MonoBehaviour
{
    private static DebugPrint ms_instance;

    private Text m_test = null;

    private List<string> m_printStr = new List<string>();

    private void Awake()
    {
        ms_instance = this;
        m_test = GetComponent<Text>();

    }

    public static void Print(string str)
    {
        if (ms_instance == null) return;

        ms_instance.m_printStr.Add(str);


    }
    private void LateUpdate()
    {
        string text = "";
        foreach(string str in m_printStr) 
        {
            text += str + "\n";
        }
        m_test.text = text;
        //1ÉtÉåÅ[ÉÄÇ≈è¡Ç¶ÇÈ
        m_printStr.Clear();
    }
}
