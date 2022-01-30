public static class GameData
{
    public const int ScoreDefaultValue = 0;
    private static int? _score = null;
    public static int Score
    {
        get
        {
            if (_score == null)
            {
                _score = ScoreDefaultValue;
                DataManager.SetValue(nameof(Score), (int)_score);
            }

            return DataManager.GetValue<int>(nameof(Score));
        }
        set
        {
            _score = value;
            DataManager.SetValue(nameof(Score), (int)_score);
        }
    }

    public const int LivesDefaultValue = 10;
    private static int? _lives = null;
    public static int Lives
    {
        get
        {
            if (_lives == null)
            {
                _lives = LivesDefaultValue;
                DataManager.SetValue(nameof(Lives), (int)_lives);
            }

            return DataManager.GetValue<int>(nameof(Lives));
        }
        set
        {
            _lives = value;
            DataManager.SetValue(nameof(Lives), (int)_lives);
        }
    }

    public static void ResetValues()
    {
        Score = ScoreDefaultValue;
        Lives = LivesDefaultValue;
    }
}
