using UnityEngine;

public class MonsterGoesToCourtEvent : Event
{
    public MonsterUI MonsterUI { get; }
    
    public MonsterGoesToCourtEvent() { }
    
    public MonsterGoesToCourtEvent(MonsterUI monsterUI)
    {
        MonsterUI = monsterUI;
    }
}