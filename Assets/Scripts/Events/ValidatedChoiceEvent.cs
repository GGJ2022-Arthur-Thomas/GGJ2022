public class ValidatedChoiceEvent : Event
{
    public bool IsRight { get; }
    
    public ValidatedChoiceEvent() { }
    
    public ValidatedChoiceEvent(bool isRight)
    {
        IsRight = isRight;
    }
}
