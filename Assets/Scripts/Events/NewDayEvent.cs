public class NewDayEvent : Event
{
    /// <summary>
    /// Between 0 (beginning of first day) and 39 (beginning of last day).
    /// </summary>
    public int DayNb { get; }

    public NewDayEvent() {}

    public NewDayEvent(int dayNb)
    {
        DayNb = dayNb;
    }
}
