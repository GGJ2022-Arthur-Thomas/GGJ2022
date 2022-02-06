public class SceneLoadingEvent : Event
{
    public string SceneName { get; }

    public SceneLoadingEvent() { }

    public SceneLoadingEvent(string sceneName)
    {
        SceneName = sceneName;
    }
}
