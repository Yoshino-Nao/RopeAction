using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gimmick1 : MonoBehaviour
{
    [SerializeField, Header("ギミック突破後の開く扉")]
    private GameObject m_Door;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "ball(Clone)")
        {
            m_Door.SetActive(false);
        }
    }
}
