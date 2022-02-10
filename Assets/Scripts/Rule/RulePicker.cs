using ExtensionMethods;
using UnityEngine;
using Folder;

public sealed class RulePicker : Singleton<RulePicker>,
    IEventHandler<NewGodRequestEvent>, IEventHandler<BulleHideAnimationEndedEvent>
{
    [Folder]
    [SerializeField]
    private string rulesFolder;
    
    private Rule[] rules;
    private int currentRuleIndex = 0;
    private bool nextRuleAnnounced = false;
    
    public Rule CurrentRule => rules[currentRuleIndex];
    public Rule NextRule => nextRuleAnnounced ? rules[currentRuleIndex + 1] : null;

    void Start()
    {
        rules = rulesFolder.LoadFolder<Rule>();
        rules.Shuffle();
        currentRuleIndex = 0;
        nextRuleAnnounced = false;
        Logger.Log(CurrentRule.Text);
        this.Subscribe<NewGodRequestEvent>();
        this.Subscribe<BulleHideAnimationEndedEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<NewGodRequestEvent>();
        this.UnSubscribe<BulleHideAnimationEndedEvent>();
        base.OnDestroy();
    }
    
    void IEventHandler<NewGodRequestEvent>.Handle(NewGodRequestEvent newGodRequestEvent)
    {
        PickNextRule();
    }

    void IEventHandler<BulleHideAnimationEndedEvent>.Handle(BulleHideAnimationEndedEvent newDayEvent)
    {
        if (nextRuleAnnounced)
            ApplyNextRule();
    }

    private void PickNextRule()
    {
        nextRuleAnnounced = true;
        this.Publish<NewRulePickedEvent>();
        Logger.Log(NextRule.Text);
    }

    private void ApplyNextRule()
    {
        nextRuleAnnounced = false;
        ++currentRuleIndex;
        if (currentRuleIndex >= rules.Length - 1)
        {
            var savedCurrentRule = CurrentRule;
            rules.Shuffle();
            rules.PutToFront(savedCurrentRule);
            currentRuleIndex = 0;
        }
        
        this.Publish<NewCurrentRuleEvent>();
        Logger.Log(CurrentRule.Text);
    }
}
