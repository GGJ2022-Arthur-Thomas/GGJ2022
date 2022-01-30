using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterQueue : MonoBehaviour,
    IEventHandler<NewMonsterArrivesEvent>
{
    [SerializeField] private float initialScale = 0.1f;
    [SerializeField] private float finalScale = 0.5f;
    [SerializeField] private float initialLightness = 0.0f;
    [SerializeField] private float finalLightness = 0.75f;
    
    [SerializeField] private GameObject monsterTemplate;
    
    [SerializeField] private Transform monsterQueueSlots;
    [SerializeField] private Transform monsterQueueSpawn;

    private List<MonsterUI> monstersInQueue = new List<MonsterUI>();

    private int slotCount = 2;

    private void Awake()
    {
        slotCount = monsterQueueSlots.childCount;
    }

    private void Start()
    {
        SpawnMonstersInQueue();
        this.Subscribe<NewMonsterArrivesEvent>();
    }

    private void SpawnMonstersInQueue()
    {
        for (var i = 0; i < slotCount; ++i)
        {
            var slot = monsterQueueSlots.GetChild(i);
            var newMonster = Instantiate(monsterTemplate, slot.position, Quaternion.identity, transform);
            var newMonsterUI = newMonster.GetComponent<MonsterUI>();
            newMonsterUI.Monster = MonsterPicker.Instance.MonsterQueue[i+1];
            var newMonsterInitialScale = Mathf.Lerp(initialScale, finalScale, (float)(slotCount - i - 1) / (slotCount - 1));
            var newMonsterInitialLightness = Mathf.Lerp(initialLightness, finalLightness, (float)(slotCount - i - 1) / (slotCount - 1));
            newMonster.GetComponent<MonsterPerspective>().SetInitialValues(newMonsterInitialScale, newMonsterInitialLightness);
            newMonster.SetActive(true);
            monstersInQueue.Add(newMonsterUI);
        }

        for (var i = 0; i < monstersInQueue.Count; ++i)
        {
            monstersInQueue[i].transform.SetSiblingIndex(slotCount + 1 - i);
        }
    }

    private void OnDestroy()
    {
        this.UnSubscribe<NewMonsterArrivesEvent>();
    }

    void IEventHandler<NewMonsterArrivesEvent>.Handle(NewMonsterArrivesEvent newMonsterArrivesEvent)
    {
        AdvanceAllMonsters();
        SpawnNewMonster();
    }

    private void AdvanceAllMonsters()
    {
        var firstMonsterInQueue = monstersInQueue[0];
        monstersInQueue.RemoveAt(0);
        
        for (var i = 0; i < monstersInQueue.Count; ++i)
        {
            monstersInQueue[i].SetPOI(monsterQueueSlots.GetChild(i).position, 1.0f);
            var newScale = Mathf.Lerp(initialScale, finalScale, (float)(slotCount - i - 1) / (slotCount - 1));
            var newLightness = Mathf.Lerp(initialLightness, finalLightness, (float)(slotCount - i - 1) / (slotCount - 1));
            monstersInQueue[i].GetComponent<MonsterPerspective>().SetFinalValues(newScale, newLightness, 1.0f);
        }
        
        this.Publish(new MonsterGoesToCourtEvent(firstMonsterInQueue));
    }

    private void SpawnNewMonster()
    {
        var newMonster = Instantiate(monsterTemplate, monsterQueueSpawn.position, Quaternion.identity, transform);
        newMonster.transform.SetSiblingIndex(2);
        var newMonsterUI = newMonster.GetComponent<MonsterUI>();
        newMonsterUI.Monster = MonsterPicker.Instance.LastMonsterInQueue;
        newMonsterUI.SetPOI(monsterQueueSlots.GetChild(slotCount - 1).position, 1.0f);
        var newMonsterPerspective = newMonster.GetComponent<MonsterPerspective>();
        newMonsterPerspective.SetInitialValues(0.0f, 0.0f);
        newMonsterPerspective.SetFinalValues(initialScale, initialLightness, 1.0f);
        newMonster.SetActive(true);
        monstersInQueue.Add(newMonsterUI);
    }
}
