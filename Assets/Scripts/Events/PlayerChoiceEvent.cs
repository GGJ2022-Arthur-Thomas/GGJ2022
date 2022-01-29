public class PlayerChoiceEvent : Event
{
    public bool IsAccepted { get; }
    
    public PlayerChoiceEvent() { }
    
    public PlayerChoiceEvent(bool isAccepted)
    {
        IsAccepted = isAccepted;
    }
}