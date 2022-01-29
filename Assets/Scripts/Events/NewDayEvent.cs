public class NewDayEvent : Event
{
    /// <summary>
    /// Between 0 (beginning of first day) and {NbDays} (set in Timeline).
    /// </summary>
    public int DayNb { get; }

    public NewDayEvent() {}

    public NewDayEvent(int dayNb)
    {
        DayNb = dayNb;
    }
}
