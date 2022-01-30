using UnityEngine;

public class EndingChooser : MonoBehaviour
{
    [Tooltip("Will be activated if win.")]
    [SerializeField] private GameObject winGameObject;

    [Tooltip("Will be activated if lose.")]
    [SerializeField] private GameObject loseGameObject;

    void Start()
    {
        bool win = GameData.Lives > 0;

        winGameObject.SetActive(win);
        loseGameObject.SetActive(!win);
    }
}
