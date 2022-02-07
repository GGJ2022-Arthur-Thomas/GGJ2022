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
        AudioManager.ChangeMusic(win ? winSettings.Music : loseSettings.Music);
        AudioManager.SetFade(fade);
    }

    [System.Serializable]
    public class EndingSettings
    {
        [SerializeField]
        private GameObject gameObject;

        [SerializeField]
        private AudioClip music;


        public GameObject GameObject => gameObject;
        public AudioClip Music => music;
    }
}
