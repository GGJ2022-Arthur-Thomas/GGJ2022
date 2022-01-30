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

    public void Replay()
    {
        GameData.ResetValues();
        SceneManager.LoadScene(Constants.MainSceneName);
    }

    public void GoToGame()
    {
        SceneManager.LoadScene(Constants.MainSceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(Constants.MainMenuSceneName);
    }
}
