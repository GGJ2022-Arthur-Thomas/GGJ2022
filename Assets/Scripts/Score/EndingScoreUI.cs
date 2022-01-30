using UnityEngine;

public class EndingScoreUI : ScoreUI
{
    [SerializeField] private float delayBetweenTicks = 0.02f;

    private int beginScore;
    private int endScore;
    private float lastTickTime;

    protected override void Start()
    {
        beginScore = 0;
        endScore = GameData.Score;
        UpdateScoreText(beginScore);
        //endScore = 250; // DEBUG
    }

    void Update()
    {
        if (beginScore == endScore)
            return;

        if (Time.time - lastTickTime > delayBetweenTicks)
        {
            Tick();
            lastTickTime = Time.time;
        }
    }

    private void Tick()
    {
        beginScore++;
        UpdateScoreText(beginScore);
    }
}
