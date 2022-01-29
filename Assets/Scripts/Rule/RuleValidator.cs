using System;
using System.Linq;
using UnityEngine;

public class RuleValidator : Singleton<RuleValidator>,
    IEventHandler<PlayerChoiceEvent>
{
    private void Start()
    {
        this.Subscribe<PlayerChoiceEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<PlayerChoiceEvent>();
        base.OnDestroy();
    }
    
    void IEventHandler<PlayerChoiceEvent>.Handle(PlayerChoiceEvent playerChoiceEvent)
    {
        this.Publish(new ValidatedChoiceEvent(IsChoiceRight(
            RulePicker.Instance.CurrentRule,
            MonsterPicker.Instance.CurrentMonster,
            playerChoiceEvent.IsAccepted)));
    }
    
    private bool IsChoiceRight(Rule rule, Monster monster, bool isAccepted)
    {
        return rule.Monsters.Contains(monster) == isAccepted;
    }
}
