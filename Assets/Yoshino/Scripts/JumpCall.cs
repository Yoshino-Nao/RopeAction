using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCall : MonoBehaviour
{
    private MoveTest m_player;
    // Start is called before the first frame update
    void Start()
    {
        m_player = transform.parent.GetComponent<MoveTest>();
    }
    private void jump()
    {
        m_player.Jump();
    }
}
