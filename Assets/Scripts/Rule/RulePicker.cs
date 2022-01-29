using System;
using UnityEngine;
using Folder;

public sealed class RulePicker : MonoBehaviour,
    IEventHandler<NewGodRequestEvent>
{
    private static readonly Lazy<RulePicker> lazy =
        new Lazy<RulePicker>(() => new RulePicker());

    public static RulePicker Instance => lazy.Value;

    private RulePicker()
    {
    }
    
    [Folder]
    [SerializeField]
    private string rulesFolder;
    
    private Rule[] rules;
    private int currentRuleIndex = 0;
    
    public Rule CurrentRule => rules[currentRuleIndex];

    void Start()
    {
        rules = rulesFolder.LoadFolder<Rule>();
        rules.Shuffle();
        currentRuleIndex = 0;
        Debug.Log(CurrentRule.Text);
        this.Subscribe<NewGodRequestEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<NewGodRequestEvent>();
    }
    
    void IEventHandler<NewGodRequestEvent>.Handle(NewGodRequestEvent newGodRequestEvent)
    {
        PickNextRule();
    }

    private void PickNextRule()
    {
        ++currentRuleIndex;
        if (currentRuleIndex >= rules.Length)
        {
            rules.Shuffle();
            currentRuleIndex = 0;
        }
        
        Debug.Log(CurrentRule.Text);
    }
}
