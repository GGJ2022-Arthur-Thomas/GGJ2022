using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Rule", fileName = "Rule")]
public class Rule : ScriptableObject
{
    [SerializeField]
    [TextArea]
    private string text;
    
    [SerializeField]
    private Monster[] monsters;


    public string Text => text;
    public Monster[] Monsters => monsters;
}
