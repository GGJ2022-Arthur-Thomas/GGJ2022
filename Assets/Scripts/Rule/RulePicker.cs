using System;
using UnityEngine;
using Folder;

public sealed class RulePicker : MonoBehaviour
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
    
    public Rule CurrentRule { get; private set; }
    
    void Start()
    {
        rules = rulesFolder.LoadFolder<Rule>();
        PickRandomRule();
        Debug.Log(CurrentRule.Text);
    }

    private void PickRandomRule()
    {
        CurrentRule = rules.GetRandomElement();
    }
}
