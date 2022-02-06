using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Audio;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/* Use :
 * - Attach this script to any GameObject or create a new one in GameObject -> Audio -> Audio Manager
 * - Fill the list in the inspector with some AudioClips
 * - To play a Sound, for example : AudioManager.PlaySound( mySoundNameWithoutFileExtension )
 * There is the same for music.
 * You can manage your own playlists direcly within the inspector, or with a bunch of functions in code.
 */

/* TODO
 * - Play Sound with randomized pitch and volume (give range, default range 0 - 1)
 * - Play Sound with a specific volume
 * - Sounds following transforms
 * - Figure what to do about MusicOnStart ( From playlist or not ? What to do with loop on start ? )
 */

[AddComponentMenu("Audio/Audio Manager")]
public class AudioManager : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/Audio/Audio Manager")]

    static void AudioManagerInstance()
    {
        GameObject go = new GameObject("Audio Manager");
        go.AddComponent<AudioManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create Audio Manager");
    }
#endif

    public List<Playlist> playlists = new List<Playlist>();

    public List<AudioClip> audioClips = new List<AudioClip>();

    public AudioSource previewChannel; // Used in Editor Mode only

    public Playlist playlistFromWhichTheClipIsPreviewed = null;

    public bool playMusicOnStart;

    [SerializeField]
    [Range(0f, 1f)]
    private float musicVolumeOnStart = 1f;

    [SerializeField]
    private bool dontDestroyOnLoad = false;

    [SerializeField]
    private string musicOnStart;

    public bool loopMusicOnStart;

    [Range(0, 16)]
    public int maxSoundsAtATime;

    public bool mustFade;
    private static bool mustFadeStatic;

    public AnimationCurve fadeIn = new AnimationCurve(
            new Keyframe[] {
                new Keyframe( 0, 0 ),
                new Keyframe( 1, 1 )
            }
        );

    public AnimationCurve fadeOut = new AnimationCurve(
            new Keyframe[] {
                new Keyframe( 0, 1 ),
                new Keyframe( 1, 0 )
            }
        );

    private static Dictionary<string, AudioClip> audioClipsDic;

    private static Dictionary<string, Playlist> playlistsDic;

    private static List<AudioSource> channels;
    private static AudioSource musicChannel; // Additional channel dedicated to music

    private static bool fadingMusic = false;
    private static bool fadingOut = false;
    private static string musicToFade;

    private static float startFadingTime;

    private static float soundVolume = 1;
    private static float musicVolume = 1;

    private static float fromFadeOutVolume = 1f;
    private static float toFadeInVolume = 1f;

    private static bool musicOn = true;
    private static bool soundOn = true;

    private static string pausedMusicName = "";

    private static AudioManager instance = null;

    public static bool InstanceInitialized
    {
        get { return instance != null; }
    }

    /// <summary>
    /// To know if music is enabled.
    /// </summary>
    public static bool IsMusicEnabled
    {
        get { return musicOn; }
    }

    /// <summary>
    /// To know if sound is enabled.
    /// </summary>
    public static bool IsSoundEnabled
    {
        get { return soundOn; }
    }

    /// <summary>
    /// To know is music is currently playing.
    /// </summary>
    public static bool MusicIsPlaying
    {
        get { return musicChannel.isPlaying; }
    }

    /// <summary>
    /// Between 0 and 1.
    /// </summary>
    public static float AppVolume
    {
        get { return AudioListener.volume; }
    }

    /// <summary>
    /// Between 0 and 1.
    /// </summary>
    public static float MusicVolume
    {
        get { return musicVolume; }
    }

    /// <summary>
    /// Between 0 and 1.
    /// </summary>
    public static float SoundVolume
    {
        get { return soundVolume; }
    }


    void Awake()
    {

        if (instance != null)
        {
            Debug.LogWarning("Only one instance of AudioManager can be run at one time !");
            return;
        }

        instance = this;

        if (previewChannel != null)
        {
            // If we played a preview while in the inspector, it created a hidden AudioSource to play the sound.
            // We want to destroy it on Awake, if it still exists (i.e. if we pushed Play without stopping the sound first)
            Destroy(previewChannel);
            // If the user still wants to preview a sound in Play mode, he can do it,
            // But is is not possible to keep the sound playing while switching between Editor and Play Mode.
        }

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        audioClipsDic = new Dictionary<string, AudioClip>();

        for (int i = 0; i < audioClips.Count; i++)
        {
            if (audioClips[i] != null)
            {
                audioClipsDic.Add(audioClips[i].name, audioClips[i]);
            }
            else
            {
                Debug.LogError(string.Format("Something gone wrong when adding clip at index {0}", i));
            }
        }

        playlistsDic = new Dictionary<string, Playlist>();
        foreach (Playlist p in playlists)
        {
            if (!playlistsDic.ContainsKey(p.title))
            {
                playlistsDic.Add(p.title, p);
            }
            else
            {
                ThrowPlaylistAlreadyExistsException(p.title);
            }
        }

        channels = new List<AudioSource>();
        for (int i = 0; i < maxSoundsAtATime; i++)
        {
            channels.Add(gameObject.AddComponent<AudioSource>());
        }

        if (audioClips.Count > 0)
        {
            musicChannel = gameObject.AddComponent<AudioSource>(); // We add an AudioSource for music if there is at least one clip in AudioManager's list
        }

        SetLoopMusic(loopMusicOnStart);

        if (playMusicOnStart && musicOnStart.Length != 0)
        {
            PlayMusic(musicOnStart, false);

            SetMusicVolume(musicVolumeOnStart);
        }

        mustFadeStatic = mustFade;

        if (playMusicOnStart && mustFade)
        {
            fadingMusic = true;
        }

        if (PlayerPrefs.HasKey("AppVolume"))
        {
            SetAppVolume(PlayerPrefs.GetFloat("AppVolume"));
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
        }

        if (PlayerPrefs.HasKey("SoundVolume"))
        {
            SetSoundVolume(PlayerPrefs.GetFloat("SoundVolume"));
        }
    }

    void Update()
    {
        mustFade = mustFadeStatic;

        if (fadingMusic)
        {
            float currentTime = Time.time - startFadingTime;
            if (fadingOut)
            {
                musicChannel.volume = fadeOut.Evaluate(currentTime) * fromFadeOutVolume;
                if (currentTime >= fadeOut.keys[fadeOut.length - 1].time)
                {
                    fadingOut = false;
                    musicChannel.Stop();
                    startFadingTime = Time.time; // Reset for fading in

                    if (!string.IsNullOrEmpty(musicToFade))
                    {
                        PlayMusic(musicToFade, true);
                    }
                }
            }
            else
            {
                // Fading In
                musicChannel.volume = fadeIn.Evaluate(currentTime) * toFadeInVolume;
                if (currentTime >= fadeIn.keys[fadeIn.length - 1].time)
                {
                    fadingMusic = false;
                    startFadingTime = 0;
                }
            }
        }
    }

    private static void ThrowNotInAudioManagerException(string soundOrMusicName)
    {
        Debug.LogError(string.Format("The clip '{0}' is not in the AudioManager's list !", soundOrMusicName));
    }

    private static void ThrowPlaylistDoesNotExistException(string playlistName)
    {
        Debug.LogError(string.Format("The playlist '{0}' does not exist !", playlistName));
    }

    private static void ThrowPlaylistAlreadyExistsException(string playlistName)
    {
        Debug.LogError(string.Format("There is already a playlist with the name '{0}' !", playlistName));
    }

    private static void ThrowPlaylistIsEmptyException(string playlistName)
    {
        Debug.LogError(string.Format("The playlist '{0}' is empty !", playlistName));
    }

    private static void ThrowClipCouldNotBeLoaded(string clipPath)
    {
        Debug.LogError(string.Format("The clip with path '{0}' could not be loaded from Resources !", clipPath));
    }

    private static void ThrowClipIsNotInPlaylist(string playlistName, string clipName)
    {
        Debug.LogError(string.Format("The clip '{0}' is not in the playlist '{1}' !", clipName, playlistName));
    }

    private static void ThrowVolumeIsNotInRange(string clipName, float volume)
    {
        Debug.LogError(string.Format("You tried to play the clip with name {0} with incorrect volume (you gave {1}) !", clipName, volume));
    }

    /// <summary>
    /// Clears the Audio Manager's list. Warning : It does not clear your playlists.
    /// </summary>
    public static void ClearAudioManagerList()
    {
        // We clear the public field to be able to see the changes in the inspector
        instance.audioClips.Clear();
        instance.audioClips = new List<AudioClip>();
        // We clear the static dictionary to get it work internally
        audioClipsDic.Clear();
        audioClipsDic = new Dictionary<string, AudioClip>();
    }

    /// <summary>
    /// Adds a clip into the AudioManager's list. Uses Resources.Load.
    /// </summary>
    /// <param name="clipPath">The path of the clip you want to add to the AudioManager's list. It has to be located somewhere in the Resources folder.</param>
    /// <returns>Returns true if the clip could be loaded from Resources; Otherwise, returns false.</returns>
    public static bool AddInAudioManagerList(string clipPath)
    {
        AudioClip c = Resources.Load<AudioClip>(clipPath);
        if (c == null)
        {
            ThrowClipCouldNotBeLoaded(clipPath);
            return false;
        }
        AddInAudioManagerList(c);
        return true;
    }

    /// <summary>
    /// Adds a set of clips into the AudioManager's list. Uses Resources.Load.
    /// </summary>
    /// <param name="clipPaths">The path of the clips you want to add to the AudioManager's list. They have to be located somewhere in the Resources folder.</param>
    /// <returns>Returns true if ALL the clips could be loaded from Resources; Otherwise, returns false.</returns>
    public static bool AddInAudioManagerList(string[] clipPaths)
    {
        bool ret = true;
        foreach (string s in clipPaths)
        {
            if (!AddInAudioManagerList(s))
            {
                ret = false;
            }
        }
        return ret;
    }

    /// <summary>
    /// Adds a clip into the AudioManager's list.
    /// </summary>
    /// <param name="clip">The clip to add to the AudioManager's list.</param>
    public static void AddInAudioManagerList(AudioClip clip)
    {
        AddInAudioManagerList(new AudioClip[] { clip });
    }

    /// <summary>
    /// Adds a set of clips into the AudioManager's list.
    /// </summary>
    /// <param name="clips">The array of clips to add to the AudioManager's list.</param>
    public static void AddInAudioManagerList(AudioClip[] clips)
    {
        foreach (AudioClip c in clips)
        {
            // Do not add if the clip is already in the list
            if (!audioClipsDic.ContainsKey(c.name))
            {
                // We add in the public field to be able to see the changes in the inspector
                instance.audioClips.Add(c);
                // We add in the static dictionary to get it work internally
                audioClipsDic.Add(c.name, c);
            }
        }
        // Sort alphabetically
        instance.audioClips = instance.audioClips.OrderBy(x => x.name).ToList();
    }

    /// <summary>
    /// Removes a clip from the AudioManager's list.
    /// </summary>
    /// <param name="clipName">The name of the clip to remove from the AudioManager's list.</param>
    public static void RemoveFromAudioManagerList(string clipName)
    {
        RemoveFromAudioManagerList(new string[] { clipName });
    }

    /// <summary>
    /// Removes a set of clips from the AudioManager's list.
    /// </summary>
    /// <param name="clipName">The name of the clips to remove from the AudioManager's list.</param>
    public static void RemoveFromAudioManagerList(string[] clipNames)
    {
        AudioClip[] clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++)
        {
            clips[i] = audioClipsDic[clipNames[i]];
        }
        RemoveFromAudioManagerList(clips);
    }

    /// <summary>
    /// Removes a clip from the AudioManager's list.
    /// </summary>
    /// <param name="clip">The clip to remove from the AudioManager's list.</param>
    public static void RemoveFromAudioManagerList(AudioClip clip)
    {
        RemoveFromAudioManagerList(new AudioClip[] { clip });
    }

    /// <summary>
    /// Removes a set of clips from the AudioManager's list.
    /// </summary>
    /// <param name="clips">The clips to remove from the AudioManager's list.</param>
    public static void RemoveFromAudioManagerList(AudioClip[] clips)
    {
        foreach (AudioClip c in clips)
        {
            // We remove from the public field to be able to see the changes in the inspector
            instance.audioClips.Remove(c);
            // We remove from the static dictionary to get it work internally
            audioClipsDic.Remove(c.name);

            // We remove the clip from all playlists
            foreach (Playlist p in instance.playlists)
            {
                for (int i = 0; i < p.sounds.Count; i++)
                {
                    if (p.sounds[i].clip == c)
                    {
                        p.sounds.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// To know if a given clip is in the AudioManager's list.
    /// </summary>
    /// <param name="clipName">The name of the clip you want to know if it's in the AudioManager's list.</param>
    /// <returns>Returns true if the clip is in the AudioManager's list; Otherwise, returns false.</returns>
    public static bool IsInAudioManagerList(string clipName)
    {
        return audioClipsDic.ContainsKey(clipName);
    }

    /// <summary>
    /// Creates a new playlist, of sounds by default. Playlists are useful for picking a random clip in the list to play.
    /// </summary>
    /// <param name="playlistName">The name of the new playlist.</param>
    /// <param name="clips">You can pass an array of clips to fill the list, or fill it later with AudioManager.AddInPlaylist().</param>
    /// <returns>Returns true if the list could be created; Otherwise, returns false.</returns>
    public static bool CreatePlaylist(string playlistName, AudioClip[] clips = null)
    {
        return CreatePlaylist(playlistName, AudioClipType.Sounds, clips);
    }

    /// <summary>
    /// Creates a new playlist. Playlists are useful for picking a random clip in the list to play.
    /// </summary>
    /// <param name="playlistName">The name of the new playlist.</param>
    /// <param name="audioClipType">Does the playlist contain sounds or musics ?</param>
    /// <param name="clips">You can pass an array of clips to fill the list, or fill it later with AudioManager.AddInPlaylist().</param>
    /// <returns>Returns true if the list could be created; Otherwise, returns false.</returns>
    public static bool CreatePlaylist(string playlistName, AudioClipType audioClipType, AudioClip[] clips = null)
    {
        if (playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistAlreadyExistsException(playlistName);
            return false;
        }
        // We add in the public field to be able to see the changes in the inspector
        instance.playlists.Add(new Playlist(playlistName, audioClipType, clips));
        // We add in the static dictionary to get it work internally
        playlistsDic.Add(playlistName, new Playlist(playlistName, audioClipType, clips));

        return true;
    }

    /// <summary>
    /// Plays a clip by name from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist where the clip is.</param>
    /// <param name="clipName">The clip to play.</param>
    /// <returns>Returns true if the clip could be played; Returns false for all other reason.</returns>
    public static bool PlayFromPlaylist(string playlistName, string clipName, bool directlyFadeIn = false)
    {
        Sound s = GetFromPlaylist(playlistName, clipName);
        if (s == null) return false;

        if (playlistsDic[playlistName].audioClipType == AudioClipType.Sounds)
        {
            return PlaySound(clipName, s.volume * playlistsDic[playlistName].volume, s.outputAudioMixerGroup);
        }
        else
        {
            float clipVolume = s.volume * playlistsDic[playlistName].volume;

            if (!mustFadeStatic)
            {
                SetMusicVolume(clipVolume);
            }
            else
            {
                fromFadeOutVolume = musicChannel.volume;
                toFadeInVolume = clipVolume;
            }

            SetMusicAudioMixerGroup(s.outputAudioMixerGroup);

            if (musicChannel.isPlaying || musicChannel.clip == null)
            {
                return ChangeMusic(clipName, directlyFadeIn);
            }
            return PlayMusic(clipName);
        }
    }

    /// <summary>
    /// Plays a random sound or music from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist.</param>
    /// <param name="randomizedPitch">Use this if you want to randomize the pitch.</param>
    /// <param name="directlyFadeIn">Used only when the playlist is of music.</param>
    /// <returns>Returns true if the playlist could be found and the sound or music could be played; Returns false for all other reason.</returns>
    public static bool PlayRandomFromPlaylist(string playlistName, bool randomizedPitch = false, float minPitch = -2, float maxPitch = 2, bool directlyFadeIn = false)
    {
        Sound randomAudio = GetRandomFromPlaylist(playlistName);
        if (randomAudio == null) return false;

        if (randomizedPitch)
        {

        }

        if (playlistsDic[playlistName].audioClipType == AudioClipType.Sounds)
        {
            return PlaySound(randomAudio.clip.name, randomAudio.volume * playlistsDic[playlistName].volume, randomAudio.outputAudioMixerGroup);
        }
        else
        {
            float clipVolume = randomAudio.volume * playlistsDic[playlistName].volume;

            if (!mustFadeStatic)
            {
                SetMusicVolume(clipVolume);
            }
            else
            {
                fromFadeOutVolume = musicChannel.volume;
                toFadeInVolume = clipVolume;
            }

            SetMusicAudioMixerGroup(randomAudio.outputAudioMixerGroup);

            return ChangeMusic(randomAudio.clip.name, directlyFadeIn);
        }
    }

    public static Sound GetFromPlaylist(string playlistName, string clipName)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return null;
        }
        int indexInPlaylist = playlistsDic[playlistName].sounds.FindIndex(c => c.clip != null && c.clip.name == clipName);
        if (indexInPlaylist == -1)
        {
            ThrowClipIsNotInPlaylist(playlistName, clipName);
            return null;
        }

        return playlistsDic[playlistName].sounds[indexInPlaylist];
    }

    public static Sound GetRandomFromPlaylist(string playlistName)
    {
        if (string.IsNullOrEmpty(playlistName) || !playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return null;
        }

        if (playlistsDic[playlistName].sounds.Count == 0)
        {
            ThrowPlaylistIsEmptyException(playlistName);
            return null;
        }

        return playlistsDic[playlistName].sounds[Random.Range(0, playlistsDic[playlistName].sounds.Count)];
    }

    /// <summary>
    /// Adds a clip into the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to add the clip in.</param>
    /// <param name="clipName">The clip to add to the playlist.</param>
    /// <returns>Returns true if the clip could be added to the playlist; Otherwise, returns false.</returns>
    public static bool AddInPlaylist(string playlistName, string clipName)
    {
        return AddInPlaylist(playlistName, audioClipsDic[clipName]);
    }

    /// <summary>
    /// Adds a set of clips into the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to add the clips in.</param>
    /// <param name="clipNames">The array of clips to add to the playlist.</param>
    /// <returns>Returns true if the clips could be added to the playlist; Otherwise, returns false.</returns>
    public static bool AddInPlaylist(string playlistName, string[] clipNames)
    {
        AudioClip[] clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++)
        {
            clips[i] = audioClipsDic[clipNames[i]];
        }
        return AddInPlaylist(playlistName, clips);
    }

    /// <summary>
    /// Adds a clip into the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to add the clip in.</param>
    /// <param name="clip">The clip to add to the playlist.</param>
    /// <returns>Returns true if the clip could be added to the playlist; Otherwise, returns false.</returns>
    public static bool AddInPlaylist(string playlistName, AudioClip clip)
    {
        return AddInPlaylist(playlistName, new AudioClip[] { clip });
    }

    /// <summary>
    /// Adds a set of clips into the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to add the clips in.</param>
    /// <param name="clips">The array of clips to add to the playlist.</param>
    /// <returns>Returns true if the clips could be added to the playlist; Otherwise, returns false.</returns>
    public static bool AddInPlaylist(string playlistName, AudioClip[] clips)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return false;
        }
        // Clips that are already in the playlist will be ignored
        Playlist p = instance.playlists.Find(tmp => tmp.title == playlistName);
        foreach (AudioClip c in clips)
        {
            if (!audioClipsDic.ContainsKey(c.name))
            {
                ThrowNotInAudioManagerException(c.name);
            }
            else if (!p.sounds.Any(x => x.clip == c))
            {
                // We add in the public field to be able to see the changes in the inspector
                p.sounds.Add(new Sound(c, 1));
                // We add in the static dictionary to get it work internally
                playlistsDic[playlistName].sounds.Add(new Sound(c, 1));
            }
        }
        return true;
    }

    /// <summary>
    /// Removes a clip from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to remove the clip from.</param>
    /// <param name="clipName">The clip to remove from the playlist.</param>
    /// <returns>Returns true if the clip could be removed from the playlist; Otherwise, returns false.</returns>
    public static bool RemoveFromPlaylist(string playlistName, string clipName)
    {
        return RemoveFromPlaylist(playlistName, audioClipsDic[clipName]);
    }

    /// <summary>
    /// Removes a set of clips from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to remove the clips from.</param>
    /// <param name="clipNames">The array of clips to remove from the playlist.</param>
    /// <returns>Returns true if the clips could be removed from the playlist; Otherwise, returns false.</returns>
    public static bool RemoveFromPlaylist(string playlistName, string[] clipNames)
    {
        AudioClip[] clips = new AudioClip[clipNames.Length];
        for (int i = 0; i < clipNames.Length; i++)
        {
            clips[i] = audioClipsDic[clipNames[i]];
        }
        return RemoveFromPlaylist(playlistName, clips);
    }

    /// <summary>
    /// Removes a clip from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to remove the clip from.</param>
    /// <param name="clip">The clip to remove from the playlist.</param>
    /// <returns>Returns true if the clip could be removed from the playlist; Otherwise, returns false.</returns>
    public static bool RemoveFromPlaylist(string playlistName, AudioClip clip)
    {
        return RemoveFromPlaylist(playlistName, new AudioClip[] { clip });
    }

    /// <summary>
    /// Removes a set of clips from the playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to remove the clips from.</param>
    /// <param name="clips">The array of clips to remove from the playlist.</param>
    /// <returns>Returns true if the clips could be removed from the playlist; Otherwise, returns false.</returns>
    public static bool RemoveFromPlaylist(string playlistName, AudioClip[] clips)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return false;
        }
        // Clips that are not in the playlist will just be ignored
        Playlist p = instance.playlists.Find(tmp => tmp.title == playlistName);
        foreach (AudioClip c in clips)
        {
            for (int i = p.sounds.Count - 1; i >= 0; i--)
            {
                if (p.sounds[i].clip == c)
                {
                    // We remove from the static dictionary to get it work internally
                    playlistsDic[playlistName].sounds.Remove(p.sounds[i]);
                }
            }
        }
        return true;
    }

    /// <summary>
    /// Changes the volume of the given playlist. Note: it does not update the volume of the preview channel.
    /// </summary>
    /// <param name="playlistName">The name of the playlist you want to change the volume.</param>
    /// <param name="newVolume">The new volume for the playlist; It has to be between 0 and 1.</param>
    /// <returns>Returns true if the playlist was found and the volume had a correct value; Otherwise, returns false.</returns>
    public static bool SetPlaylistVolume(string playlistName, float newVolume)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return false;
        }
        if (newVolume < 0 || newVolume > 1)
        {
            Debug.LogError(string.Format("You tried to set the volume on playlist {0} with an incorrect value (you gave {1}) !", playlistName, newVolume));
            return false;
        }
        playlistsDic[playlistName].volume = newVolume;
        return true;
    }

    /// <summary>
    /// Sets the given AudioMixerGroup on the given clip in the given playlist.
    /// </summary>
    /// <param name="playlistName">The name of the playlist where the clip is located.</param>
    /// <param name="clipName">The name of the clip on which you want to set the AudioMixerGroup.</param>
    /// <param name="audioMixerGroup">The AudioMixerGroup you want to set on the clip.</param>
    /// <returns>Returns true if the AudioMixerGroup could be set on the clip; Returns false for all other reason.</returns>
    public static bool SetAudioMixerGroupOnClipFromPlaylist(string playlistName, string clipName, AudioMixerGroup audioMixerGroup)
    {
        return SetAudioMixerGroupOnClipFromPlaylist(playlistName, audioClipsDic[clipName], audioMixerGroup);
    }

    /// <summary>
    /// Sets the given AudioMixerGroup on the given clip in the given playlist.
    /// </summary>
    /// <param name="playlistName">The name of the playlist where the clip is located.</param>
    /// <param name="clip">The clip on which you want to set the AudioMixerGroup.</param>
    /// <param name="audioMixerGroup">The AudioMixerGroup you want to set on the clip.</param>
    /// <returns>Returns true if the AudioMixerGroup could be set on the clip; Returns false for all other reason.</returns>
    public static bool SetAudioMixerGroupOnClipFromPlaylist(string playlistName, AudioClip clip, AudioMixerGroup audioMixerGroup)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return false;
        }

        Sound sound = playlistsDic[playlistName].sounds.Find(s => s.clip == clip);

        if (sound == null)
        {
            ThrowClipIsNotInPlaylist(playlistName, clip.name);
            return false;
        }

        sound.outputAudioMixerGroup = audioMixerGroup;

        return true;
    }

    /// <summary>
    /// Removes the given AudioMixerGroup from the given clip in the given playlist.
    /// </summary>
    /// <param name="playlistName">The name of the playlist where the clip is located.</param>
    /// <param name="clipName">The name of the clip from which you want to remove the AudioMixerGroup.</param>
    /// <returns>Returns true if the AudioMixerGroup could be removed from the clip; Returns false for all other reason.</returns>
    public static bool RemoveAudioMixerGroupOnClipFromPlaylist(string playlistName, string clipName)
    {
        return SetAudioMixerGroupOnClipFromPlaylist(playlistName, audioClipsDic[clipName], null);
    }

    /// <summary>
    /// Removes the given AudioMixerGroup from the given clip in the given playlist.
    /// </summary>
    /// <param name="playlistName">The name of the playlist where the clip is located.</param>
    /// <param name="clip">The clip from which you want to remove the AudioMixerGroup.</param>
    /// <returns>Returns true if the AudioMixerGroup could be removed from the clip; Returns false for all other reason.</returns>
    public static bool RemoveAudioMixerGroupOnClipFromPlaylist(string playlistName, AudioClip clip)
    {
        return SetAudioMixerGroupOnClipFromPlaylist(playlistName, clip, null);
    }

    /// <summary>
    /// Removes a playlist with the given name.
    /// </summary>
    /// <param name="playlistName">The name of the playlist to remove.</param>
    /// <returns>Return true if the list could be removed; Otherwise, returns false.</returns>
    public static bool RemovePlaylist(string playlistName)
    {
        if (!playlistsDic.ContainsKey(playlistName))
        {
            ThrowPlaylistDoesNotExistException(playlistName);
            return false;
        }
        // We remove from the public field to be able to see the changes in the inspector
        for (int i = instance.playlists.Count - 1; i >= 0; i--)
        {
            if (instance.playlists[i].title == playlistName)
            {
                instance.playlists.RemoveAt(i);
                break;
            }
        }
        // We remove from the static dictionary to get it work internally
        playlistsDic.Remove(playlistName);
        return true;
    }

    /// <summary>
    /// Removes all playlists.
    /// </summary>
    public static void RemoveAllPlaylists()
    {
        // We remove from the public field to be able to see the changes in the inspector
        instance.playlists.Clear();
        instance.playlists = new List<Playlist>();
        // We remove from the static dictionary to get it work internally
        playlistsDic.Clear();
        playlistsDic = new Dictionary<string, Playlist>();
    }

    /// <summary>
    /// Plays a music from its name in the AudioManager's list.
    /// </summary>
    /// <param name="musicName">The name of the music to play. It has to be in the AudioManager's list.</param>
    /// <param name="forceReplay">If set to true, the music will restart from the beginning if it was already playing.</param>
    /// <returns>Returns true if the music could be played; Returns false for all other reason.</returns>
    public static bool PlayMusic(string musicName, bool forceReplay = false)
    {
        if (!audioClipsDic.ContainsKey(musicName))
        {
            ThrowNotInAudioManagerException(musicName);
            return false;
        }
        if (!musicOn)
        {
            // If the music is disabled, we still update the variable for when we will re-enable it
            pausedMusicName = musicName;
        }
        if (musicChannel.clip == null || musicChannel.clip.name != musicName && musicOn || forceReplay)
        {
            // We play the music only if it is different from the current one, to avoid replay,
            // Unless we absolutely wanted to replay (forceReplay)
            musicChannel.clip = GetClipByName(musicName);
            musicChannel.Play();
        }
        return musicChannel.isPlaying;
    }

    /// <summary>
    /// Plays a music on Start of the Game.
    /// </summary>
    /// <param name="clipName">The name of the clip you want to play.</param>
    /// <param name="mustFade">Should we use the fading curves defined in Audio Manager Editor?</param>
    /// <param name="fromPlaylist">If this is true, it will play the music with the settings defined in the playlist.</param>
    /// <param name="playlistName">If fromPlaylist is true, you must specify from which playlist the clip is.</param>
    /// <returns>Returns true if the music could be found and played; Returns false for all other reason.</returns>
    public static bool PlayMusicOnStart(string clipName, bool mustFade, bool fromPlaylist, string playlistName = "", bool loop = false)
    {
        if (!audioClipsDic.ContainsKey(clipName))
        {
            ThrowNotInAudioManagerException(clipName);
            return false;
        }
        mustFadeStatic = mustFade;
        SetLoopMusic(loop);
        if (fromPlaylist)
        {
            if (!playlistsDic.ContainsKey(playlistName))
            {
                ThrowPlaylistDoesNotExistException(playlistName);
                return false;
            }
            int indexInPlaylist = playlistsDic[playlistName].sounds.FindIndex(c => c.clip != null && c.clip.name == clipName);
            if (indexInPlaylist == -1)
            {
                ThrowClipIsNotInPlaylist(playlistName, clipName);
                return false;
            }
            Sound s = playlistsDic[playlistName].sounds[indexInPlaylist];
            SetMusicVolume(s.volume * playlistsDic[playlistName].volume);
            SetMusicAudioMixerGroup(s.outputAudioMixerGroup);
            return ChangeMusic(clipName, true);
        }
        return ChangeMusic(clipName);
    }

    /// <summary>
    /// Fades out the music using the fading curves defined in AudioManager.
    /// </summary>
    public static void FadeOutMusic()
    {
        if (!fadingMusic)
        {
            startFadingTime = Time.time;
        }
        fromFadeOutVolume = musicChannel.volume;
        musicToFade = "";
        fadingMusic = true;
        fadingOut = true;
    }

    /// <summary>
    /// Shortcut for <see cref="ChangeMusic(string, bool)"/>. Uses <paramref name="musicClip"/>.name.
    /// </summary>
    public static bool ChangeMusic(AudioClip musicClip, bool directlyFadeIn = false)
    {
        return ChangeMusic(musicClip.name, directlyFadeIn);
    }

    /// <summary>
    /// Switches from the current music to a new one. It may use the fading curves defined in AudioManager.
    /// </summary>
    /// <param name="musicName">The name of the music to play. It has to be in the AudioManager's list.</param>
    /// <returns>Returns true if the music could be found; Otherwise, returns false.</returns>
    public static bool ChangeMusic(string musicName, bool directlyFadeIn = false)
    {
        if (!audioClipsDic.ContainsKey(musicName))
        {
            ThrowNotInAudioManagerException(musicName);
            return false;
        }
        if (!mustFadeStatic)
        {
            musicChannel.Stop();
            PlayMusic(musicName, true);
        }
        else
        {
            if (!fadingMusic)
            {
                startFadingTime = Time.time;
            }
            musicToFade = musicName;
            fadingMusic = true;
            if (directlyFadeIn)
            {
                fadingOut = false;
                PlayMusic(musicToFade, true);
            }
            else
            {
                fadingOut = true;
            }
        }
        return true;
    }

    /// <summary>
    /// Randomizes the time of the music. Useful to create a new experience every time you play the music.
    /// </summary>
    public static void SetMusicRandomTime()
    {
        musicChannel.time = Random.Range(0, musicChannel.clip.length);
    }

    /// <summary>
    /// Sets if changing music should use a fade transition using the fading curves defined in AudioManager.
    /// </summary>
    /// <param name="newValue">If set to true, changing music will use the fading curves defined in AudioManager.</param>
    /// <returns>Returns true if the parameter has been set correctly; Otherwise, returns false.</returns>
    public static bool SetFade(bool newValue)
    {
        if (fadingMusic)
        {
            Debug.LogWarning("You cannot change the fade while fading already !");
            return false;
        }
        mustFadeStatic = newValue;
        return true;
    }

    /// <summary>
    /// Gets an AudioClip from its name in the AudioManager's list.
    /// </summary>
    /// <param name="soundOrMusicName">The name of the sound or the music you want the AudioClip. It has to be in the AudioManager's list.</param>
    /// <returns>Returns the clip if it has been found; Otherwise, returns null.</returns>
    public static AudioClip GetClipByName(string soundOrMusicName)
    {
        if (audioClipsDic.ContainsKey(soundOrMusicName))
        {
            return audioClipsDic[soundOrMusicName];
        }
        else
        {
            ThrowNotInAudioManagerException(soundOrMusicName);
            return null;
        }
    }

    /// <summary>
    /// Stops the music.
    /// </summary>
    public static void StopMusic()
    {
        if (musicChannel != null) musicChannel.Stop();
        fadingMusic = false; // If we don't do this, and we are fading, it will ignore stopping the music...
    }

    /// <summary>
    /// Disables the music.
    /// </summary>
    /// <param name="pause">If set to true, the music will pause instead of stop.</param>
    public static void DisableMusic(bool pause)
    {
        if (pause)
        {
            PauseMusic();
        }
        else
        {
            musicChannel.Stop();
            musicOn = false;
        }
    }

    /// <summary>
    /// Enables the music.
    /// </summary>
    /// <param name="unPause">If set to true, and music was disabled with pause parameter set to true, the music will unpause instead of start from the beginning.</param>
    public static void EnableMusic(bool unPause)
    {
        if (unPause)
        {
            UnPauseMusic();
        }
        else
        {
            musicOn = true;
            PlayMusic(pausedMusicName);
        }
    }

    /// <summary>
    /// Pauses the music.
    /// </summary>
    public static void PauseMusic()
    {
        if (musicChannel != null && musicChannel.clip != null)
        {
            pausedMusicName = musicChannel.clip.name;
        }
        musicChannel.Pause();
        musicOn = false;
    }

    /// <summary>
    /// Unpauses the music.
    /// </summary>
    public static void UnPauseMusic()
    {
        if (musicChannel.clip == null || string.IsNullOrEmpty(pausedMusicName)) return;

        if (pausedMusicName == musicChannel.clip.name)
        {
            musicChannel.UnPause();
        }
        else
        {
            musicOn = true;
            PlayMusic(pausedMusicName);
        }
    }

    /// <summary>
    /// Sets the music's pitch.
    /// </summary>
    /// <param name="pitchValue">This parameter has the same properties as any AudioSource's Pitch parameter.</param>
    public static void SetMusicPitch(float pitchValue)
    {
        musicChannel.pitch = pitchValue;
    }

    /// <summary>
    /// Sets the music volume. Notes: use <paramref name="applyInPlaylists"/> to also apply this change to music playlists. It does not update the volume of the preview channel.
    /// </summary>
    /// <param name="newVolume">The volume of the music in your app. It has to be between 0 and 1.</param>
    public static void SetMusicVolume(float newVolume, bool applyInPlaylists = false)
    {
        if (newVolume < 0 || newVolume > 1)
        {
            Debug.LogError(string.Format("You tried to set music volume to an incorrect value (you gave {0}) !", newVolume));
            return;
        }

        if (fadingMusic)
        {
            fromFadeOutVolume = musicChannel.volume;
            toFadeInVolume = newVolume;
            //Debug.LogWarning( "Music is already fading and prevents from changing volume !" );
            //return;
        }

        musicVolume = newVolume;
        musicChannel.volume = musicVolume;
        PlayerPrefs.SetFloat("MusicVolume", newVolume);

        if (applyInPlaylists)
        {
            SetAllPlaylistsVolume(newVolume, AudioClipType.Musics);
        }
    }

    /// <summary>
    /// Sets the volume of all playlists with the specified <see cref="AudioClipType"/>. 
    /// </summary>
    /// <param name="newVolume">The volume you want to set. It has to be between 0 and 1.</param>
    /// <param name="clipType">Choose whether you want to set the volume for musics, or sounds playlists.</param>
    public static void SetAllPlaylistsVolume(float newVolume, AudioClipType clipType)
    {
        if (playlistsDic != null)
        {
            foreach (KeyValuePair<string, Playlist> kvp in playlistsDic)
            {
                if (kvp.Value.audioClipType == clipType)
                {
                    SetPlaylistVolume(kvp.Key, newVolume);
                }
            }
        }
    }

    /// <summary>
    /// Sets the music's Audio Mixer Group.
    /// </summary>
    /// <param name="group">This parameter has the same properties as any AudioSource's Output parameter.</param>
    public static void SetMusicAudioMixerGroup(AudioMixerGroup group)
    {
        musicChannel.outputAudioMixerGroup = group;
    }

    public static bool PlaySound(string soundName, float volume, float panStereo)
    {
        if (PlaySound(soundName, volume))
        {
            foreach (AudioSource channel in channels)
            {
                if (channel.clip != null && channel.clip.name == soundName && channel.time == 0)
                {
                    channel.panStereo = panStereo;
                }
            }
            
            return true;
        }

        return false;
    }

    /// <summary>
    /// From a base of 1, if pitchAmplitude = 0.5, will randomize pitch between 0.5 and 1.5.
    /// </summary>
    public static bool PlaySoundWithRandomPitch(string soundName, float volume, float pitchAmplitude)
    {
        return PlaySoundWithRandomPitch(soundName, volume, 1 - pitchAmplitude, 1 + pitchAmplitude);
    }

    public static bool PlaySoundWithRandomPitch(string soundName, float volume, float minPitch = -3f, float maxPitch = 3f)
    {
        return PlaySoundWithPitch(soundName, volume, Random.Range(minPitch, maxPitch));
    }

    public static bool PlaySoundWithPitch(string soundName, float volume, float pitch)
    {
        if (PlaySound(soundName, volume))
        {
            foreach (AudioSource channel in channels)
            {
                if (channel.clip != null && channel.clip.name == soundName && channel.time == 0)
                {
                    channel.pitch = pitch;
                }
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Plays a sound from its name in the AudioManager's list.
    /// </summary>
    /// <param name="soundName">The name of the sound to play. It has to be in the AudioManager's list.</param>
    /// <param name="volume">The volume you want to set on the sound to be played. It has to be between 0 and 1. Note: it does not update the volume of the preview channel.</param>
    /// <param name="loop">If set to true, the sound will loop.</param>
    /// <param name="group">The AudioMixerGroup you want to set on the sound to be played.</param>
    /// <returns>Returns true if the sound could be played; Returns false for all other reason.</returns>
    public static bool PlaySound(string soundName, float volume = 1f, bool loop = false, AudioMixerGroup group = null)
    {
        if (!audioClipsDic.ContainsKey(soundName))
        {
            ThrowNotInAudioManagerException(soundName);
            return false;
        }
        if (volume < 0 || volume > 1)
        {
            ThrowVolumeIsNotInRange(soundName, volume);
            return false;
        }
        foreach (AudioSource channel in channels)
        {
            if (!channel.isPlaying)
            {
                channel.panStereo = 0f; // Reset pan
                channel.pitch = 1f; // Reset pitch
                channel.clip = GetClipByName(soundName);
                channel.loop = loop;
                channel.volume = volume * SoundVolume;
                channel.outputAudioMixerGroup = group;
                if (soundOn)
                {
                    channel.Play();
                }
                return true;
            }
        }
        Debug.LogWarning(string.Format("All channels are occupied ! Sound '{0}' could not be played.", soundName));
        return false;
    }

    /// <summary>
    /// Sets the global sounds volume. Notes: use <paramref name="applyInPlaylists"/> to also apply this change to sound playlists. It does not update the volume of the preview channel.
    /// </summary>
    /// <param name="newValue">The volume you want to set on all sounds. It has to be between 0 and 1</param>
    public static void SetSoundVolume(float newVolume, bool applyInPlaylists = false)
    {
        if (newVolume < 0 || newVolume > 1)
        {
            Debug.LogError(string.Format("You tried to set all sounds volume to an incorrect value (you gave {0}) !", newVolume));
            return;
        }

        soundVolume = newVolume;
        PlayerPrefs.SetFloat("SoundVolume", newVolume);

        foreach (AudioSource channel in channels)
        {
            channel.volume = soundVolume;
        }

        if (applyInPlaylists)
        {
            SetAllPlaylistsVolume(newVolume, AudioClipType.Sounds);
        }
    }

    /// <summary>
    /// Use this to control the whole app volume. Saves the value in PlayerPrefs with name "AppVolume".
    /// </summary>
    /// <param name="newVolume">Has to be between 0 and 1.</param>
    public static void SetAppVolume(float newVolume)
    {
        if (newVolume < 0 || newVolume > 1)
        {
            Debug.LogError(string.Format("You tried to set app volume to an incorrect value (you gave {0}) !", newVolume));
            return;
        }

        AudioListener.volume = newVolume;
        PlayerPrefs.SetFloat("AppVolume", newVolume);
    }

    /// <summary>
    /// To know if a sound or a music is currently playing.
    /// </summary>
    /// <param name="soundOrMusicName">The name of the sound or the music you want to know the playing state.</param>
    /// <returns>Returns true if the sound or the music is playing; Returns false for all other reason.</returns>
    public static bool IsPlaying(string soundOrMusicName)
    {
        if (!audioClipsDic.ContainsKey(soundOrMusicName))
        {
            ThrowNotInAudioManagerException(soundOrMusicName);
            return false;
        }
        // It's a sound
        foreach (AudioSource channel in channels)
        {
            if (channel.isPlaying && channel.clip.name == soundOrMusicName)
            {
                return true;
            }
        }
        // It's a music
        return (musicChannel.clip.name == soundOrMusicName && musicChannel.isPlaying);
    }

    public static void StopAllSounds()
    {
        foreach (AudioSource channel in channels)
        {
            channel.Stop();
        }
    }

    /// <summary>
    /// Stops the sound from its name in the AudioManager's list.
    /// </summary>
    /// <param name="soundName">The name of the sound you want to stop. It has to be in the AudioManager's list.</param>
    /// <returns>Returns true if the sound could be stopped; Returns false for all other reason.</returns>
    public static bool StopSound(string soundName)
    {
        if (!audioClipsDic.ContainsKey(soundName))
        {
            ThrowNotInAudioManagerException(soundName);
            return false;
        }
        foreach (AudioSource channel in channels)
        {
            if (channel.isPlaying && channel.clip.name == soundName)
            {
                channel.loop = false;
                channel.Stop();
                return true;
            }
        }
        // The sound was found but was not playing
        return false;
    }

    /// <summary>
    /// Set looping on music.
    /// </summary>
    /// <param name="loop"></param>
    public static void SetLoopMusic(bool loop)
    {
        if (musicChannel != null) musicChannel.loop = loop;
    }

    /// <summary>
    /// Set looping on sound from its name in the AudioManager's list.
    /// </summary>
    /// <param name="soundOrMusicName"></param>
    /// <param name="loop">If set to true, the sound or music will loop.</param>
    /// <returns>Returns true if the sound or music could be stop looped; Returns false for all other reason.</returns>
    public static bool SetLoopSound(string soundOrMusicName, bool loop)
    {
        if (!audioClipsDic.ContainsKey(soundOrMusicName))
        {
            ThrowNotInAudioManagerException(soundOrMusicName);
            return false;
        }
        foreach (AudioSource channel in channels)
        {
            if (channel.loop && channel.clip.name == soundOrMusicName)
            {
                channel.loop = loop;
                return true;
            }
        }
        return true;
    }

    /// <summary>
    /// Enables the sounds.
    /// </summary>
    public static void EnableSound()
    {
        soundOn = true;
    }

    /// <summary>
    /// Disables the sounds.
    /// </summary>
    public static void DisableSound()
    {
        soundOn = false;
    }
}

public enum AudioClipType
{
    Sounds,
    Musics
}

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public float volume;
    public AudioMixerGroup outputAudioMixerGroup;

    public Sound(AudioClip c, float v = 1, AudioMixerGroup a = null)
    {
        clip = c;
        volume = v;
        outputAudioMixerGroup = a;
    }
}

[System.Serializable]
public class Playlist
{
    public string title;
    public AudioClipType audioClipType;
    public List<Sound> sounds;
    public float volume;
    public bool isFolded;

    public Playlist(string t, AudioClipType a, List<AudioClip> c = null, float v = 1)
    {
        title = t;
        audioClipType = a;
        sounds = new List<Sound>();
        if (c != null)
        {
            foreach (AudioClip clip in c)
            {
                sounds.Add(new Sound(clip));
            }
        }
        volume = v;
        isFolded = false;
    }

    public Playlist(string t, AudioClipType a, AudioClip[] c)
        : this(t, a, c == null ? null : c.ToList())
    {

    }
}