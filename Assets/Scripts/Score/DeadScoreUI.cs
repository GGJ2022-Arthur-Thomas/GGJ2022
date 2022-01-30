using TMPro;
using UnityEngine;

public class DeadScoreUI : MonoBehaviour
{
    [SerializeField] private TMP_Text message;
    
    // Start is called before the first frame update
    void Start()
    {
        message.text = "VOUS ÊTES JETÉS DE L'ARCHE!!!\n" +
                       "Vous avez tout de même sauvé " + ScoreManager.Score + " animaux <3";
    }
}
