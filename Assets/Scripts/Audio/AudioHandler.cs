using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioHandler : Singleton<AudioHandler>,
    IEventHandler<SceneLoadingEvent>
{
    [SerializeField]
    private string startingSceneName;

    [SerializeField]
    private List<SceneMusicSettings> sceneMusics;

    void Start()
    {
        if (!string.IsNullOrEmpty(startingSceneName))
        {
            AudioManager.SetFade(false);
            PlaySceneMusic(startingSceneName);
            AudioManager.SetFade(true);
        }

        this.Subscribe<SceneLoadingEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<SceneLoadingEvent>();
        base.OnDestroy();
    }

    void IEventHandler<SceneLoadingEvent>.Handle(SceneLoadingEvent @event)
    {
        PlaySceneMusic(@event.SceneName);
    }

    private void PlaySceneMusic(string sceneName)
    {
        SceneMusicSettings musicPair = sceneMusics.FirstOrDefault(p => p.SceneName == sceneName);

        if (musicPair != null)
        {
            AudioManager.SetMusicVolume(musicPair.Volume);
            AudioManager.ChangeMusic(musicPair.MusicClip);
        }
    }

    [System.Serializable]
    class SceneMusicSettings
    {
        [SerializeField]
        private string sceneName;

        [SerializeField]
        private AudioClip musicClip;

        [SerializeField]
        [Range(0f, 1f)]
        private float volume = 1f;

        public string SceneName => sceneName;
        public AudioClip MusicClip => musicClip;
        public float Volume => volume;
    }
}
