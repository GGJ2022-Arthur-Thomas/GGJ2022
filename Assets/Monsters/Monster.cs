using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Monster", fileName = "Monster")]
public class Monster : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite sprite;
    [SerializeField] [TextArea] private string description;
    
    public Sprite Sprite => sprite;
    public string DisplayName => displayName;
    public string Description => description;
}
