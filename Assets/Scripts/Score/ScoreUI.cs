using UnityEngine;

public class ScoreUI : MonoBehaviour,
    IEventHandler<ScoreChangedEvent>
{
    [SerializeField] private TMP_ParamText scoreText;
    
    protected virtual void Start()
    {
        UpdateScoreText(GameData.Score);
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

    protected void UpdateScoreText(int score)
    {
        scoreText.SetText(score.ToString());
    }
}