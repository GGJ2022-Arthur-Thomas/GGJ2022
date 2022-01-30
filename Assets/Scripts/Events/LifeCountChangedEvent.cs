public class LifeCountChangedEvent : Event
{
    public int NewAmount { get; }

    public LifeCountChangedEvent() {}

    public LifeCountChangedEvent(int newAmount)
    {
        NewAmount = newAmount;
    }
}
