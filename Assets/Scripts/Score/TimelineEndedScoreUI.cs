using TMPro;
using UnityEngine;

public class TimelineEndedScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text message;
    
    // Start is called before the first frame update
    void Start()
    {
        message.text = "FÉLICITATIONS!!!\n" +
                       "Après tant de jours de tri, vous avez pu sauver " + ScoreManager.Score + " animaux B-)";
    }
}
