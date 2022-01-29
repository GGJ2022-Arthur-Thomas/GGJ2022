using System;
using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour,
    IEventHandler<ScoreChangedEvent>
{
    [SerializeField] private TMP_Text scoreText;
    
    private void Start()
    {
        UpdateScoreText();
        this.Subscribe<ScoreChangedEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<ScoreChangedEvent>();
    }

    void IEventHandler<ScoreChangedEvent>.Handle(ScoreChangedEvent scoreChangedEvent)
    {
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Score: " + ScoreManager.Instance.Score;
    }
}