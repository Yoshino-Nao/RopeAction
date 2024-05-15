using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReStart : MonoBehaviour
{
    [SerializeField] private string m_sceneName;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag== "Player")
        {
            SceneManager.LoadScene(m_sceneName);
        }
    }
}
