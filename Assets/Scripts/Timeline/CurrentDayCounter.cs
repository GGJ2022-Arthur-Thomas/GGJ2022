using UnityEngine;
using TMPro;

public class CurrentDayCounter : MonoBehaviour,
    IEventHandler<NewDayEvent>
{
    [SerializeField] private TMP_Text text;

    void Awake()
    {
        this.Subscribe<NewDayEvent>();
    }

    void OnDestroy()
    {
        this.UnSubscribe<NewDayEvent>();
    }

    void IEventHandler<NewDayEvent>.Handle(NewDayEvent @event)
    {
        text.text = $"Jour {@event.DayNb + 1}";
    }
}
