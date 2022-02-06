using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AudioHandler : Singleton<AudioHandler>,
    IEventHandler<SceneLoadingEvent>
{
    [SerializeField]
    private List<SceneMusicSettings> sceneMusics;

    void Start()
    {
        this.Subscribe<SceneLoadingEvent>();
    }

    protected override void OnDestroy()
    {
        this.UnSubscribe<SceneLoadingEvent>();
        base.OnDestroy();
    }

    void IEventHandler<SceneLoadingEvent>.Handle(SceneLoadingEvent @event)
    {
        SceneMusicSettings musicPair = sceneMusics.FirstOrDefault(p => p.SceneName == @event.SceneName);

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
