using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour
{
    [SerializeField] private Monster monsterData;

    [SerializeField] private TMP_Text monsterNameText;
    [SerializeField] private Image monsterImage;
    [SerializeField] private TMP_Text monsterDescriptionText;

    void Start()
    {
        SetMonster(monsterData);
    }

    private void OnValidate()
    {
        UpdateUI();
    }

    private void SetMonster(Monster monsterData)
    {
        this.monsterData = monsterData;
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
