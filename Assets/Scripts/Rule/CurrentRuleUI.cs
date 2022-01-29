using TMPro;
using UnityEngine;

public class CurrentRuleUI : MonoBehaviour,
    IEventHandler<NewCurrentRuleEvent>
{
    [SerializeField] private TMP_Text currentRuleText;

    bool initialized = false;

    void Start()
    {
        currentRuleText.text = "";
        this.Subscribe<NewCurrentRuleEvent>();
    }

    void OnDestroy()
    {
        this.UnSubscribe<NewCurrentRuleEvent>();
    }

    void Update()
    {
        if (!initialized) // initialize the first frame it's possible
        {
            UpdateUI();
            initialized = true;
        }
    }

    void IEventHandler<NewCurrentRuleEvent>.Handle(NewCurrentRuleEvent @event)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        currentRuleText.text = RulePicker.Instance.CurrentRule.Text;
    }
}
