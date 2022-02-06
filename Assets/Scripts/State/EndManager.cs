public class EndManager : Singleton<EndManager>,
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
    
    void IEventHandler<DeadEvent>.Handle(DeadEvent @event)
    {
        GoToEnd();
    }

    void IEventHandler<TimelineEndedEvent>.Handle(TimelineEndedEvent @event)
    {
        GoToEnd();
    }

    private void GoToEnd()
    {
        SceneLoader.GoToEnd();
    }
}
