using UnityEngine;

public class EndingChooser : MonoBehaviour
{
    [SerializeField]
    private EndingSettings winSettings;

    [SerializeField]
    private EndingSettings loseSettings;

    void Start()
    {
        bool win = GameData.Lives > 0;

        winSettings.GameObject.SetActive(win);
        loseSettings.GameObject.SetActive(!win);

        bool fade = AudioManager.GetFade();
        AudioManager.SetFade(false);
        EndingSettings settings = win ? winSettings : loseSettings;
        AudioManager.ChangeMusic(settings.Sound, directlyFadeIn: true);
        AudioManager.SetFade(fade);
    }

    [System.Serializable]
    public class EndingSettings
    {
        [SerializeField]
        private GameObject gameObject;

        [SerializeField]
        private AudioClip sound;


        public GameObject GameObject => gameObject;
        public AudioClip Sound => sound;
    }
}
