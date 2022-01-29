using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour,
    IEventHandler<NewMonsterArrivesEvent>
{
    [SerializeField] private int monsterQueueOffset = 0;

    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private TMP_Text monsterDescriptionText;

    private Monster monsterData;
    
    private void Start()
    {
        UpdateMonster();
        this.Subscribe<NewMonsterArrivesEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<NewMonsterArrivesEvent>();
    }

    private void OnValidate()
    {
        UpdateUI();
    }

    void IEventHandler<NewMonsterArrivesEvent>.Handle(NewMonsterArrivesEvent newMonsterArrivesEvent)
    {
        UpdateMonster();
    }
    
    private void UpdateMonster()
    {
        this.monsterData = MonsterPicker.Instance.MonsterQueue[monsterQueueOffset];
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (monsterData == null)
            return;
        
        monsterNameText.text = monsterData.DisplayName;
        monsterImage.sprite = monsterData.Sprite;
        monsterDescriptionText.text = monsterData.Description;
    }
}
