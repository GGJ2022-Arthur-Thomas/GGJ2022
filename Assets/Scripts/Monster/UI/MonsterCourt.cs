using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterCourt : MonoBehaviour,
    IEventHandler<PlayerChoiceEvent>,
    IEventHandler<MonsterGoesToCourtEvent>
{
    [SerializeField] private GameObject monsterTemplate;
    
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private TMP_Text monsterDescriptionText;
    
    [SerializeField] private Transform acceptedDestination;
    [SerializeField] private Transform rejectedDestination;

    [SerializeField] private Transform monstersParent;

    [SerializeField] private float addMonsterDuration = 0.3f;
    [SerializeField] private float acceptDuration = 0.3f; 
    [SerializeField] private float rejectDuration = 0.5f; 
    
    private List<MonsterUI> monstersInCourt = new List<MonsterUI>();
    private List<MonsterUI> monstersLeavingCourt = new List<MonsterUI>();

    private MonsterUI CurrentMonster => monstersInCourt[0];

    private void Start()
    {
        InitMonster();
        this.Subscribe<PlayerChoiceEvent>();
        this.Subscribe<MonsterGoesToCourtEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<PlayerChoiceEvent>();
        this.UnSubscribe<MonsterGoesToCourtEvent>();
    }

    private void OnValidate()
    {
        UpdateUI();
    }

    private void Update()
    {
        for (var i = monstersLeavingCourt.Count - 1; i >= 0; --i)
        {
            if (!monstersLeavingCourt[i].HasArrived())
                continue;
            
            Destroy(monstersLeavingCourt[i].gameObject);
            monstersLeavingCourt.RemoveAt(i);
        }
    }

    void IEventHandler<PlayerChoiceEvent>.Handle(PlayerChoiceEvent playerChoiceEvent)
    {
        SortCurrentMonster(playerChoiceEvent.IsAccepted);
    }

    void IEventHandler<MonsterGoesToCourtEvent>.Handle(MonsterGoesToCourtEvent monsterGoesToCourtEvent)
    {
        AddMonster(monsterGoesToCourtEvent.MonsterUI);
    }

    private void InitMonster()
    {
        var newMonster = Instantiate(monsterTemplate, monstersParent);
        var newMonsterUI = newMonster.GetComponent<MonsterUI>();
        newMonsterUI.Monster = MonsterPicker.Instance.CurrentMonster;
        newMonster.SetActive(true);
        monstersInCourt.Add(newMonsterUI);
        UpdateUI();
    }
    
    private void SortCurrentMonster(bool isAccepted)
    {
        if (CurrentMonster == null)
            return;

        if (isAccepted)
        {
            CurrentMonster.SetPOI(acceptedDestination.position, acceptDuration);
        }
        else
        {
            CurrentMonster.SetPOI(rejectedDestination.position, rejectDuration);
        }
        
        monstersLeavingCourt.Add(CurrentMonster);
        monstersInCourt.RemoveAt(0);
        UpdateUI();
    }
    
    private void AddMonster(MonsterUI newMonster)
    {
        newMonster.transform.parent = monstersParent;
        newMonster.transform.SetSiblingIndex(4);
        newMonster.SetPOI(transform.position, addMonsterDuration);
        newMonster.gameObject.GetComponent<MonsterPerspective>().SetFinalValues(1.0f, 1.0f, addMonsterDuration);
        monstersInCourt.Add(newMonster);
    }

    private void UpdateUI()
    {
        if (monstersInCourt.Count == 0)
            return;
        
        monsterNameText.text = CurrentMonster.Monster.DisplayName;
        monsterDescriptionText.text = CurrentMonster.Monster.Description;
    }
}
