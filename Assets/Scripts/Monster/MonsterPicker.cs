using System;
using UnityEngine;
using Folder;

public sealed class MonsterPicker : MonoBehaviour
{
    private static readonly Lazy<MonsterPicker> lazy =
        new Lazy<MonsterPicker>(() => new MonsterPicker());

    public static MonsterPicker Instance => lazy.Value;

    private MonsterPicker()
    {
    }
    
    [Folder]
    [SerializeField]
    private string monstersFolder;
    
    private Monster[] monsters;
    
    public Monster CurrentMonster { get; private set; }
    
    void Start()
    {
        monsters = monstersFolder.LoadFolder<Monster>();
        PickRandomMonster();
        Debug.Log(CurrentMonster.DisplayName);
    }

    private void PickRandomMonster()
    {
        CurrentMonster = monsters.GetRandomElement();
    }
}
