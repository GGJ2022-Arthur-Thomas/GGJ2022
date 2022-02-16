using ExtensionMethods;
using UnityEngine;
using Folder;
using System.Linq;

public sealed class RulePicker : Singleton<RulePicker>,
    IEventHandler<BulleHideAnimationEndedEvent>
{
    [Folder]
    [SerializeField]
    private string rulesFolder;
    
    private RuleSO[] rules;
    private int currentRuleIndex = 0;
    
    public RuleSO CurrentRule => rules[currentRuleIndex];
    public RuleSO NextRule => rules[currentRuleIndex + 1];

    void Start()
    {
        rules = rulesFolder.LoadFolder<RuleSO>();
        ShuffleRules();

        Logger.Log(CurrentRule.Text);

        this.Subscribe<BulleHideAnimationEndedEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<BulleHideAnimationEndedEvent>();

        base.OnDestroy();
    }

    void IEventHandler<BulleHideAnimationEndedEvent>.Handle(BulleHideAnimationEndedEvent newDayEvent)
    {
        PickNewRule();
    }

    private void PickNewRule()
    {
        currentRuleIndex++;
        if (currentRuleIndex >= rules.Length - 1)
        {
            var savedCurrentRule = CurrentRule;
            ShuffleRules();
            PutRuleToFront(savedCurrentRule);
            currentRuleIndex = 0;
        }
        
        this.Publish<NewCurrentRuleEvent>();
        Logger.Log(CurrentRule.Text);
    }

    private void ShuffleRules()
    {
        var rnd = new System.Random();
        rules = rules.OrderBy(item => rnd.Next()).ToArray();
    }

    private void PutRuleToFront(RuleSO rule)
    {
        rules = rules.OrderBy(item => !item.Equals(rule)).ToArray();  // OrderBy is stable
    }
}
