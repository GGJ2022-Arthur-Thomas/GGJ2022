using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public const string GameSceneName = "Game";
    public const string MainMenuSceneName = "MainMenu";
    public const string CreditsSceneName = "Credits";
    public const string EndSceneName = "End";

    public static void GoToGame()
    {
        LoadSceneAsync(GameSceneName);
    }

    public static void GoToMainMenu()
    {
        LoadSceneAsync(MainMenuSceneName);
    }

    public static void GoToCredits()
    {
        LoadSceneAsync(CreditsSceneName);
    }

    public static void GoToEnd()
    {
        LoadSceneAsync(EndSceneName);
    }

    private static void LoadSceneAsync(string sceneName)
    {
        StaticCoroutine.DoCoroutine(LoadSceneAsyncCorout(sceneName));
    }

    private static IEnumerator LoadSceneAsyncCorout(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        StaticEvents.Publish(typeof(SceneLoader), new SceneLoadingEvent(sceneName));

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
