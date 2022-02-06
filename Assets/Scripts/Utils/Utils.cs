using UnityEngine;
using UnityEngine.SceneManagement;

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
        SceneManager.LoadScene(Constants.GameSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.MainMenuSceneName);
    }

    public void GoToCredits()
    {
        SceneManager.LoadScene(Constants.CreditsSceneName);
    }
}
