public class StateMachine : Singleton<StateMachine>
{
    public State CurrentState { get; private set; } = State.None;

    void Start()
    {
        ChangeState(State.Start);
    }
    
    private void ChangeState(State newState)
    {
        State oldState = CurrentState;
        CurrentState = newState;
        this.Publish(new StateChangedEvent(oldState, newState));
    }
}
