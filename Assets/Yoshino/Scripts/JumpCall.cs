using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpCall : MonoBehaviour
{
    private MoveTest m_player;
    private Player player;
    // Start is called before the first frame update
    void Start()
    {
        m_player = transform.parent.GetComponent<MoveTest>();
        player = transform.parent.GetComponent<Player>();

    }
    private void jump()
    {
        m_player?.Jump();
        player?.Jump();
    }
}
