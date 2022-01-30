using TMPro;
using UnityEngine;

public class TimelineEndedScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    
    void Start()
    {
        scoreText.text = GameData.Score.ToString();
    }
}
