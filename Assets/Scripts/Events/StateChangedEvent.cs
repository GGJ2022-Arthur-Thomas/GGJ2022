public class StateChangedEvent : Event
{
    public State OldState { get; }
    public State NewState { get; }
    
    public StateChangedEvent() { }
    
    public StateChangedEvent(State oldState, State newState)
    {
        OldState = oldState;
        NewState = newState;
    }
}
