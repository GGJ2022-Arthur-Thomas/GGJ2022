using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class LifeCountUI : MonoBehaviour,
    IEventHandler<LifeCountChangedEvent>
{
    [SerializeField] private TMP_Text lifeCountText;
    
    private void Start()
    {
        UpdateScoreText();
        this.Subscribe<LifeCountChangedEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<LifeCountChangedEvent>();
    }

    void IEventHandler<LifeCountChangedEvent>.Handle(LifeCountChangedEvent lifeCountChangedEvent)
    {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        lifeCountText.text = "Lives: " + string.Concat(Enumerable.Repeat("<3 ", ScoreManager.Lives));
    }
}
