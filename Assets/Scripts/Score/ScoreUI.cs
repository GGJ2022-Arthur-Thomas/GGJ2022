using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour,
    IEventHandler<ScoreChangedEvent>
{
    [SerializeField] private TMP_Text scoreText;
    
    private void Start()
    {
        UpdateScoreText(0);
        this.Subscribe<ScoreChangedEvent>();
    }

    private void OnDestroy()
    {
        this.UnSubscribe<ScoreChangedEvent>();
    }

    void IEventHandler<ScoreChangedEvent>.Handle(ScoreChangedEvent @event)
    {
        UpdateScoreText(@event.NewAmount);
    }

    private void UpdateScoreText(int score)
    {
        scoreText.text = score.ToString();
    }
}