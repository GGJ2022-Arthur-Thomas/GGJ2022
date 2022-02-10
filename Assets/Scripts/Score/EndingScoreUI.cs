using UnityEngine;

public class EndingScoreUI : ScoreUI
{
    [SerializeField]
    private float scoreIncreaseDuration = 2f;

    private int currentScore;
    private int endScore;

    private float totalDuration;

    protected override void Start()
    {
        currentScore = 0;
        UpdateScoreText(currentScore);
        endScore = GameData.Score;
        //endScore = 650; // DEBUG
    }

    void Update()
    {
        if (totalDuration == scoreIncreaseDuration)
            return;

        totalDuration += Time.deltaTime;

        currentScore = (int)((totalDuration / scoreIncreaseDuration) * endScore);

        if (totalDuration >= scoreIncreaseDuration)
        {
            totalDuration = scoreIncreaseDuration;
            currentScore = endScore;
        }

        UpdateScoreText(currentScore);
    }
}
