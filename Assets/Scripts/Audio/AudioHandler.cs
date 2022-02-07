using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioHandler : Singleton<AudioHandler>,
    IEventHandler<SceneLoadingEvent>,
    IEventHandler<NewGodRequestEvent>
{
    [Header("Scenes")]

    [SerializeField]
    private string startingSceneName;

    [SerializeField]
    private List<SceneMusicSettings> sceneMusics;

    [Header("God")]

    [SerializeField]
    private AudioSettings godIntervention;

    void Start()
    {
        if (!string.IsNullOrEmpty(startingSceneName))
        {
            AudioManager.SetFade(false);
            PlaySceneMusic(startingSceneName);
            AudioManager.SetFade(true);
        }

        this.Subscribe<SceneLoadingEvent>();
        this.Subscribe<NewGodRequestEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<NewGodRequestEvent>();
        this.UnSubscribe<SceneLoadingEvent>();
        base.OnDestroy();
    }

    void IEventHandler<SceneLoadingEvent>.Handle(SceneLoadingEvent @event)
    {
        PlaySceneMusic(@event.SceneName);
    }

    void IEventHandler<NewGodRequestEvent>.Handle(NewGodRequestEvent @event)
    {
        PlaySound(godIntervention);
    }

    private void PlaySceneMusic(string sceneName)
    {
        SceneMusicSettings musicPair = sceneMusics.FirstOrDefault(p => p.SceneName == sceneName);

        if (musicPair != null)
        {
            AudioManager.SetMusicVolume(musicPair.Volume);
            AudioManager.ChangeMusic(musicPair.Clip);
        }
    }

    private void PlaySound(AudioSettings settings)
    {
        AudioManager.PlaySound(settings.Clip.name, settings.Volume);
    }

    [System.Serializable]
    class SceneMusicSettings : AudioSettings
    {
        [SerializeField]
        private string sceneName;

        public string SceneName => sceneName;
    }

    [System.Serializable]
    class AudioSettings
    {
        [SerializeField]
        private AudioClip clip;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 1f;

        public AudioClip Clip => clip;
        public float Volume => volume;
    }
}
