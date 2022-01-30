public class ScoreChangedEvent : Event
{
    public int NewAmount { get; }

    public ScoreChangedEvent() {}

    public ScoreChangedEvent(int newAmount)
    {
        NewAmount = newAmount;
    }
}
