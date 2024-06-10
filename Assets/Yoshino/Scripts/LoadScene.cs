using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    [SerializeField] private GameObject Game;
    [SerializeField] private GameObject Opening;
    public void OpenScene(string name)
    {
        Debug.Log("Movie is Done");
        if (name != "")
        {
            SceneManager.LoadScene(name);
        }
    }
    public void GameStart()
    {
        Game.SetActive(true);
        Opening.SetActive(false);
    }
}
