using System;
using UnityEngine;
using Folder;

public sealed class MonsterPicker : Singleton<MonsterPicker>
{
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
