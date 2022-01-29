using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour,
    IEventHandler<NewMonsterArrivesEvent>
{
    [SerializeField] private int monsterQueueOffset = 0;

    [SerializeField] private bool showText = false;
    [SerializeField] private float darkness = 0.0f;
    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private TMP_Text monsterDescriptionText;

    private Monster monsterData;
    
    private void Start()
    {
        InitUI();
        UpdateMonster();
        this.Subscribe<NewMonsterArrivesEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<NewMonsterArrivesEvent>();
    }

    private void OnValidate()
    {
        InitUI();
        UpdateUI();
    }

    void IEventHandler<NewMonsterArrivesEvent>.Handle(NewMonsterArrivesEvent newMonsterArrivesEvent)
    {
        UpdateMonster();
    }
    
    private void InitUI()
    {
        if (!showText)
        {
            monsterNameText.gameObject.SetActive(false);
            monsterDescriptionText.gameObject.SetActive(false);
        }
        
        monsterImage.color = new Color(1.0f - darkness, 1.0f - darkness, 1.0f - darkness);
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
