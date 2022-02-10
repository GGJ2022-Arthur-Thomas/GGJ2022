using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Folder;

public sealed class MonsterPicker : Singleton<MonsterPicker>,
    IEventHandler<NewMonsterNeededEvent>
{
    [Folder]
    [SerializeField]
    private string monstersFolder;
    
    private MonsterSO[] monsters;

    private const int MONSTER_QUEUE_SIZE = 5;
    private const int MONSTER_QUEUE_PICK_MAX_RETRIES = 5;

    public List<MonsterSO> MonsterQueue { get; private set; } = new List<MonsterSO>();
    public MonsterSO CurrentMonster => MonsterQueue[0];
    public MonsterSO LastMonsterInQueue => MonsterQueue[MONSTER_QUEUE_SIZE-1];

    void Start()
    {
        monsters = monstersFolder.LoadFolder<MonsterSO>();
        InitMonsterQueue();
        Logger.Log(CurrentMonster.DisplayName);
        this.Subscribe<NewMonsterNeededEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<NewMonsterNeededEvent>();
        base.OnDestroy();
    }
    
    void IEventHandler<NewMonsterNeededEvent>.Handle(NewMonsterNeededEvent newMonsterNeededEvent)
    {
        MoveQueueForward();
    }

    private void InitMonsterQueue()
    {
        foreach (var i in Enumerable.Range(1, MONSTER_QUEUE_SIZE))
        {
            AddRandomUniqueMonsterToQueue();
        }
    }

    private void MoveQueueForward()
    {
        AddRandomUniqueMonsterToQueue();
        MonsterQueue.RemoveAt(0);
        Logger.Log("New current monster is: " + CurrentMonster.DisplayName);
        this.Publish<NewMonsterArrivesEvent>();
    }

    private void AddRandomUniqueMonsterToQueue()
    {
        var pick = monsters.GetRandomElement();
        for (var i = 1; MonsterQueue.Contains(pick) && i < MONSTER_QUEUE_PICK_MAX_RETRIES; ++i)
        {
            pick = monsters.GetRandomElement();
        }
        MonsterQueue.Add(pick);
        Logger.Log("Added " + pick.DisplayName + " at the end of the queue!");
    }
}
