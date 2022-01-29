public class LifeCountChangedEvent : Event
{
    /// <summary>
    /// Usually -1, but could be different in special occasions
    /// </summary>
    public int Amount { get; } = -1;

    public LifeCountChangedEvent() {}

    public LifeCountChangedEvent(int amount)
    {
        Amount = amount;
    }
}
