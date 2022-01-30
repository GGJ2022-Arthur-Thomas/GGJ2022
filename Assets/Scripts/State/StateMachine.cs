using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StateMachine : Singleton<StateMachine>,
    IEventHandler<DeadEvent>, IEventHandler<TimelineEndedEvent>
{
    void Start()
    {
        this.Subscribe<DeadEvent>();
        this.Subscribe<TimelineEndedEvent>();
    }
    
    protected override void OnDestroy()
    {
        this.UnSubscribe<DeadEvent>();
        this.UnSubscribe<TimelineEndedEvent>();
        base.OnDestroy();
    }
    
    void IEventHandler<DeadEvent>.Handle(DeadEvent deadEvent)
    {
        StartCoroutine(LoadDeadSceneAsync());
    }

    private IEnumerator LoadDeadSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Dead");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
    
    void IEventHandler<TimelineEndedEvent>.Handle(TimelineEndedEvent timelineEndedEvent)
    {
        StartCoroutine(LoadTimelineEndedSceneAsync());
    }

    private IEnumerator LoadTimelineEndedSceneAsync()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("TimelineEnded");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
