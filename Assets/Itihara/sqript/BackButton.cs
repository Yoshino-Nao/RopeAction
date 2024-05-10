using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButton : MonoBehaviour
{
    [SerializeField] private string m_sceneName;

     public void Onclick()
    {
        SceneManager.LoadScene(m_sceneName);
    }
}
