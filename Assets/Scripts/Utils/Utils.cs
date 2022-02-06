using UnityEngine;

public class Utils : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void GoToGame()
    {
        GameData.ResetValues();
        SceneLoader.GoToGame();
    }

    public void GoToMainMenu()
    {
        SceneLoader.GoToMainMenu();
    }

    public void GoToCredits()
    {
        SceneLoader.GoToCredits();
    }

    public void PlayClickSound()
    {
        AudioManager.PlaySound("click");
    }
}
