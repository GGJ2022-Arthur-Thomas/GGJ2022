public class ScoreChangedEvent : Event
{
    /// <summary>
    /// Usually +1, but could be different in special occasions
    /// </summary>
    public int Amount { get; } = 1;

    public ScoreChangedEvent() {}

    public ScoreChangedEvent(int amount)
    {
        Amount = amount;
    }
}
