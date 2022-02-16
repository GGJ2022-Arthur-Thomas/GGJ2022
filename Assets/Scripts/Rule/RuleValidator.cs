using System.Linq;

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
    
    void IEventHandler<PlayerChoiceEvent>.Handle(PlayerChoiceEvent @event)
    {
        Logger.Log("Player decided to " + (@event.IsAccepted ? "accept" : "reject"));
        this.Publish(new ValidatedChoiceEvent(IsChoiceRight(
            RulePicker.Instance.CurrentRule,
            MonsterPicker.Instance.CurrentMonster,
            @event.IsAccepted)));
    }
    
    private bool IsChoiceRight(RuleSO rule, MonsterSO monster, bool isAccepted)
    {
        return rule.Monsters.Contains(monster) == isAccepted;
    }
}
