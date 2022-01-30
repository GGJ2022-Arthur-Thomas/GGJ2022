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
        LoadEndSceneAsync();
    }

    void IEventHandler<TimelineEndedEvent>.Handle(TimelineEndedEvent timelineEndedEvent)
    {
        LoadEndSceneAsync();
    }

    private void LoadEndSceneAsync()
    {
        StartCoroutine(LoadSceneAsync("End"));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
