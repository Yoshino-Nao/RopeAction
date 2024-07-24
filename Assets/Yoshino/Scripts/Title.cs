using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    TimelineController m_controller;
    [SerializeField] private Canvas TitleCanvas;
    // Start is called before the first frame update
    void Awake()
    {
        m_controller = FindObjectOfType<TimelineController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (TitleCanvas.enabled == true)
            {
                TitleCanvas.enabled = false;
            }
            if (m_controller != null)
            {
                m_controller.StartTimeLine();
                Debug.Log("Timeline Start");
            }
        }
        
    }
}
