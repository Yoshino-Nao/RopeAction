using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector m_director;

    // Start is called before the first frame update
    void Awake()
    {


    }

    public void StartTimeLine()
    {
        m_director.Play();
    }
    bool IsTimeDone()
    {
        return m_director.time >= m_director.duration;
    }
    private void Update()
    {
        if (IsTimeDone())
        {
            Debug.Log("Timeline is Done");
        }
    }
}
