using UnityEditor;
using Rotorz.ReorderableList;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;
using System;

/* TODO
 * - Drag directly file or folder to playlist ?
 * - Responsive GUI in playlist ?
 * - Reorganize playlists
 * - Save on Foldout change
 * - Groups of playlists
 * - Fade between musics in preview
 */

/* KNOWN BUGS
 * - Playlist volume slider doesn't show up
 * - Foldout playlists doesn't work : make boxes
 * - Can't drag to AudioMixerGroup
 * - Slider over Volume amount and Remove button
 * - Can't edit sliders with keyboard
 * - ArgumentOutOfRangeException when 2 elements in playlist and dragging 2nd on top (sometimes)
 * - On Play it automatically foldouts everything
 */

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{

    #region Classes
    public class AddClipToPlaylist
    {
        public Playlist playlist;
        public ReorderableListControl listControl;
        public string clipName;

        public AddClipToPlaylist(Playlist playlist, ReorderableListControl listControl, string clipName)
        {
            this.playlist = playlist;
            this.listControl = listControl;
            this.clipName = clipName;
        }
    }
    #endregion

    private List<AudioClip> _soundsToAdd = new List<AudioClip>();
    private AudioClip _soundToRemove = null;

    // We need SerializedProperty on all variables we want to be saved when quitting Unity or changing scene
    // Plus, it automatically handles Undo
    SerializedProperty _dontDestroyOnLoad;
    SerializedProperty _playMusicOnStart;
    SerializedProperty _musicVolumeOnStart;
    SerializedProperty _musicOnStart;
    SerializedProperty _loopMusicOnStart;
    SerializedProperty _maxSoundsAtATime;
    SerializedProperty _mustFade;
    SerializedProperty _fadeIn;
    SerializedProperty _fadeOut;

    AudioManager audioManager;

    bool showAudioManagerList = true;
    bool showGeneralParameters = true;
    bool showMusicParameters = true;
    bool showSoundParameters = true;
    bool showPlaylists = true;

    Playlist currentIteratedPlaylist;

    float previewedClipVolume = 1;

    ReorderableListControl _currentListControl;
    List<ReorderableListControl> _listControls;
    List<IReorderableListAdaptor> _listAdaptors;

    void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        _dontDestroyOnLoad = serializedObject.FindProperty("dontDestroyOnLoad");
        _playMusicOnStart = serializedObject.FindProperty("playMusicOnStart");
        _musicVolumeOnStart = serializedObject.FindProperty("musicVolumeOnStart");
        _musicOnStart = serializedObject.FindProperty("musicOnStart");
        _loopMusicOnStart = serializedObject.FindProperty("loopMusicOnStart");
        _maxSoundsAtATime = serializedObject.FindProperty("maxSoundsAtATime");
        _mustFade = serializedObject.FindProperty("mustFade");
        _fadeIn = serializedObject.FindProperty("fadeIn");
        _fadeOut = serializedObject.FindProperty("fadeOut");

        _listControls = new List<ReorderableListControl>();
        _listAdaptors = new List<IReorderableListAdaptor>();

        audioManager = target as AudioManager;

        // We are forced to re-create the lists every time we enable the script, otherwise they are not saved
        foreach (Playlist p in audioManager.playlists)
        {
            // Create new ListControl
            _listControls.Add(new ReorderableListControl(ReorderableListFlags.DisableContextMenu | ReorderableListFlags.HideAddButton));
            if (p.sounds.Count != audioManager.audioClips.Count)
            {
                // We hide the Add button if the playlist is full
                _listControls[_listControls.Count - 1].AddMenuClicked += OnAddMenuClicked;
            }
            _listControls[_listControls.Count - 1].ItemRemoving += OnItemRemoving;
            _listControls[_listControls.Count - 1].ItemMoved += OnItemMoved;

            // Create new ListAdaptor
            _listAdaptors.Add(new GenericListAdaptor<Sound>(p.sounds, DrawSound, 16));
        }
    }

    // These 2 functions to get toggleOnLabelClick with GUILayout
    private bool Foldout(bool foldout, GUIContent content, bool toggleOnLabelClick)
    {
        Rect position = GUILayoutUtility.GetRect(40f, 40f, 16f, 16f);
        // EditorGUI.kNumberW == 40f but is internal
        return EditorGUI.Foldout(position, foldout, content, toggleOnLabelClick);
    }
    private bool Foldout(bool foldout, string content, bool toggleOnLabelClick)
    {
        return Foldout(foldout, new GUIContent(content), toggleOnLabelClick);
    }

    private Sound DrawSound(Rect position, Sound s)
    {
        int indexInPlaylist = currentIteratedPlaylist.sounds.FindIndex(x => x == s);
        if (indexInPlaylist != -1)
        {
            // We want to be able to preview even when playing
            GUI.enabled = true;
            // Preview buttons
            if (audioManager.previewChannel != null && audioManager.previewChannel.clip == s.clip && audioManager.previewChannel.isPlaying && audioManager.playlistFromWhichTheClipIsPreviewed == currentIteratedPlaylist)
            {
                GUI.color = new Color(1, 0.64f, 0.11f);
                GUI.enabled = true;

                if (GUI.Button(new Rect(position.x, position.y, 20, 16), new GUIContent("■", "Stop"), EditorStyles.miniButton))
                {
                    StopPreviewChannel();
                }
            }
            else
            {
                GUI.color = new Color(0, 0.545f, 1);
                bool playAudioClip = GUI.Button(new Rect(position.x, position.y, 20, 16), new GUIContent("►", "Play"), EditorStyles.miniButton);
                GUI.color = Color.white;
                if (playAudioClip)
                {
                    audioManager.playlistFromWhichTheClipIsPreviewed = currentIteratedPlaylist;
                    SetPreviewChannel(s.clip, s.volume, s.outputAudioMixerGroup);
                }
            }

            GUI.enabled = !Application.isPlaying;

            position.x += 22; // 20 is button's width, + 2px of margin

            GUI.color = Color.white;

            // We display the name of the AudioClip
            EditorGUI.LabelField(position, s.clip.name);

            float originalWidth = position.width; // For responsive GUI

            // We display the AudioMixerGroup
            position.x = originalWidth - 165;
            position.y++;
            position.width = 70;
            AudioMixerGroup newAudioMixerGroup = EditorGUI.ObjectField(position, currentIteratedPlaylist.sounds[indexInPlaylist].outputAudioMixerGroup, typeof(AudioMixerGroup), false) as AudioMixerGroup;
            if (newAudioMixerGroup != currentIteratedPlaylist.sounds[indexInPlaylist].outputAudioMixerGroup)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                                                                                  // Update the preview channel volume if we are previewing this clip
                if (currentIteratedPlaylist == audioManager.playlistFromWhichTheClipIsPreviewed && currentIteratedPlaylist.sounds[indexInPlaylist].clip == audioManager.previewChannel.clip)
                {
                    audioManager.previewChannel.outputAudioMixerGroup = newAudioMixerGroup;
                }
            }
            currentIteratedPlaylist.sounds[indexInPlaylist].outputAudioMixerGroup = newAudioMixerGroup;

            // We display the volume
            position.x = originalWidth - 75;
            position.width = 115; // Minimal width to display the slider
            float newVolume = EditorGUI.Slider(position, currentIteratedPlaylist.sounds[indexInPlaylist].volume, 0, 1);

            if (newVolume != currentIteratedPlaylist.sounds[indexInPlaylist].volume)
            {
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                // Update the preview channel volume if we are previewing this clip
                if (currentIteratedPlaylist == audioManager.playlistFromWhichTheClipIsPreviewed && audioManager.previewChannel != null && currentIteratedPlaylist.sounds[indexInPlaylist].clip == audioManager.previewChannel.clip)
                {
                    UpdatePreviewChannelVolume(audioManager.playlistFromWhichTheClipIsPreviewed.volume, newVolume);
                }
            }
            currentIteratedPlaylist.sounds[indexInPlaylist].volume = newVolume;
        }
        return s;
    }

    private void OnItemRemoving(object sender, ItemRemovingEventArgs args)
    {
        if (currentIteratedPlaylist.sounds.Count == audioManager.audioClips.Count)
        {
            // If the playlist was full, removing an item allows it to add a new sound
            (sender as ReorderableListControl).AddMenuClicked += OnAddMenuClicked;
        }
        if (audioManager.previewChannel != null && currentIteratedPlaylist == audioManager.playlistFromWhichTheClipIsPreviewed && currentIteratedPlaylist.sounds[args.ItemIndex].clip == audioManager.previewChannel.clip)
        {
            StopPreviewChannel();
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
    }

    private void OnItemMoved(object sender, ItemMovedEventArgs args)
    {
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
    }

    private void OnAddMenuClicked(object sender, AddMenuClickedEventArgs args)
    {
        _currentListControl = sender as ReorderableListControl;
        GenericMenu menu = new GenericMenu();
        for (int i = 0; i < audioManager.audioClips.Count; i++)
        {
            if (!currentIteratedPlaylist.sounds.Any(x => x.clip.name == audioManager.audioClips[i].name))
            {
                menu.AddItem(new GUIContent(audioManager.audioClips[i].name),
                              false,
                              OnSelectAddMenuItem,
                              new AddClipToPlaylist(currentIteratedPlaylist, _currentListControl, audioManager.audioClips[i].name)
                            );
            }
        }
        menu.DropDown(args.ButtonPosition);
    }

    private void OnSelectAddMenuItem(object userData)
    {
        AddClipToPlaylist addClipToPlaylist = (userData as AddClipToPlaylist);

        addClipToPlaylist.playlist.sounds.Add(new Sound(audioManager.audioClips.Find(x => x.name == addClipToPlaylist.clipName)));

        if (addClipToPlaylist.playlist.sounds.Count == audioManager.audioClips.Count)
        {
            // We hide the Add button if the playlist is full
            addClipToPlaylist.listControl.AddMenuClicked -= OnAddMenuClicked;
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
    }

    void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        // Remove the Add event on all listControls. Good practice, saves performance
        foreach (ReorderableListControl listControl in _listControls)
        {
            listControl.AddMenuClicked -= OnAddMenuClicked;
            listControl.ItemRemoving -= OnItemRemoving;
            listControl.ItemMoved -= OnItemMoved;
        }
    }

    void OnEditorUpdate()
    {
        // This will destroy the AudioSource as soon as the sound preview is finished
        if (audioManager != null && audioManager.previewChannel != null && !audioManager.previewChannel.isPlaying)
        {
            DestroyImmediate(audioManager.previewChannel);
        }
    }

    public void CreateNewPlaylist(string title, AudioClip[] clips = null)
    {
        if (audioManager.playlists.Any(p => p.title == title))
        {
            Debug.LogError(string.Format("A playlist with the name '{0}' already exists !", title));
            return;
        }
        // By default, it is a playlist of sounds (it is more likely)
        audioManager.playlists.Add(new Playlist(title, AudioClipType.Sounds));

        // Create new ListControl
        _listControls.Add(new ReorderableListControl(ReorderableListFlags.DisableContextMenu | ReorderableListFlags.HideAddButton));
        _listControls[_listControls.Count - 1].AddMenuClicked += OnAddMenuClicked;
        _listControls[_listControls.Count - 1].ItemRemoving += OnItemRemoving;
        _listControls[_listControls.Count - 1].ItemMoved += OnItemMoved;

        // Create new ListAdaptor
        _listAdaptors.Add(new GenericListAdaptor<Sound>(audioManager.playlists[audioManager.playlists.Count - 1].sounds, DrawSound, 16));

        Playlist newPlaylist = audioManager.playlists[audioManager.playlists.Count - 1];

        if (clips != null)
        {
            // Filling the playlist with eventual clips
            foreach (AudioClip c in clips)
            {
                newPlaylist.sounds.Add(new Sound(audioManager.audioClips.Find(x => x.name == c.name)));
                if (newPlaylist.sounds.Count == audioManager.audioClips.Count)
                {
                    // We hide the Add button if the playlist is full
                    _listControls[_listControls.Count - 1].AddMenuClicked -= OnAddMenuClicked;
                    break;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
        showPlaylists = true;
    }

    public void RenamePlaylist(string playlistPreviousName, string playlistNewName)
    {
        if (audioManager.playlists.Any(p => p.title == playlistNewName))
        {
            Debug.LogError(string.Format("A playlist with the name '{0}' already exists !", playlistNewName));
            return;
        }
        audioManager.playlists.Find(p => p.title == playlistPreviousName).title = playlistNewName;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
    }

    public bool IsInAudioManagerList(string clipName)
    {
        return audioManager.audioClips.Any(c => c.name == clipName);
    }

    void DrawEmptyPlaylist()
    {
        GUIStyle s = EditorStyles.centeredGreyMiniLabel;
        s.fixedHeight = 20;
        GUILayout.Label("No AudioClips in the playlist.", s);
    }

    void SetPreviewChannel(AudioClip clip, float volume, AudioMixerGroup group)
    {
        // First time we create the preview channel
        if (audioManager.previewChannel == null)
        {
            audioManager.previewChannel = audioManager.gameObject.AddComponent<AudioSource>();
            // We hide the preview channel so that it looks cleaner
            audioManager.previewChannel.hideFlags = HideFlags.HideInInspector;
        }
        audioManager.previewChannel.clip = clip;
        // If the clip is previewed from a playlist
        if (audioManager.playlistFromWhichTheClipIsPreviewed != null)
        {
            UpdatePreviewChannelVolume(audioManager.playlistFromWhichTheClipIsPreviewed.volume, volume);
        }
        else
        {
            UpdatePreviewChannelVolume(-1, volume);
        }
        audioManager.previewChannel.outputAudioMixerGroup = group;
        audioManager.previewChannel.Play();
    }

    /// <summary>
    /// Updates the volume of AudioManager's preview channel. Takes the volume of the playlist into account if the previewed clip is part of a playlist
    /// </summary>
    /// /// <param name="playlistVolume">Volume of the playlist, if the clip is part of a playlist</param>
    /// <param name="soundVolume">Volume of the previewed clip</param>
    void UpdatePreviewChannelVolume(float playlistVolume = -1, float soundVolume = -1)
    {
        if (audioManager.previewChannel != null)
        {
            if (soundVolume == -1)
            {
                // The clip volume has not been given, we just want to update the volume of the playlist, so we keep the current previewed clip volume
                audioManager.previewChannel.volume = previewedClipVolume;
            }
            else
            {
                audioManager.previewChannel.volume = soundVolume;
                previewedClipVolume = soundVolume;
            }
            // If the sound is played from a playlist, we apply the playlist volume
            if (playlistVolume != -1)
            {
                audioManager.previewChannel.volume *= playlistVolume;
            }
            else
            {
                audioManager.previewChannel.volume = 1;
            }
        }
    }

    void StopPreviewChannel()
    {
        if (audioManager != null && audioManager.previewChannel != null)
        {
            audioManager.previewChannel.Stop();
            DestroyImmediate(audioManager.previewChannel);
        }
    }

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        audioManager = target as AudioManager;

        EditorGUILayout.Space();

        bool wasEnabled = GUI.enabled;

        if (!Application.isPlaying)
        {
            DragAndDropAudioManagerList();
        }

        // We display things only if we're not dragging anything (otherwise, it causes errors)
        if (Event.current.type != EventType.DragUpdated && Event.current.type != EventType.DragPerform && Event.current.type != EventType.DragExited)
        {
            if (audioManager.audioClips.Count > 0)
            {
                // Remove deleted audio assets
                for (int i = audioManager.audioClips.Count - 1; i >= 0; i--)
                {
                    if (audioManager.audioClips[i] == null)
                    {
                        audioManager.audioClips.RemoveAt(i);
                    }
                }

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = true;

                showAudioManagerList = Foldout(showAudioManagerList, "Audio Manager's list", true);

                GUI.enabled = !Application.isPlaying;

                if (GUILayout.Button("Clear", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog("Clear AudioManager's list ?", "Do you really want to clear the AudioManager's list ?\nIt will clear your playlists, but not remove them !", "Yes", "No"))
                    {
                        audioManager.audioClips.Clear();
                        audioManager.audioClips = new List<AudioClip>();
                        for (int i = 0; i < audioManager.playlists.Count; i++)
                        {
                            audioManager.playlists[i].sounds.Clear();
                            audioManager.playlists[i].sounds = new List<Sound>();
                            _listControls[i].AddMenuClicked -= OnAddMenuClicked;
                            _listControls[i].ItemRemoving -= OnItemRemoving;
                            _listControls[i].ItemMoved -= OnItemMoved;
                        }
                        StopPreviewChannel();
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (showAudioManagerList)
                {
                    EditorGUILayout.Space();

                    foreach (AudioClip s in audioManager.audioClips)
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (audioManager.previewChannel != null && audioManager.previewChannel.clip == s && audioManager.previewChannel.isPlaying &&
                                string.IsNullOrEmpty(audioManager.playlistFromWhichTheClipIsPreviewed.title)) // Don't know why but I have to test if the title is null and not the playlist itself, otherwise it does not work
                        {
                            GUI.color = new Color(1, 0.64f, 0.11f);
                            // We want to be able to preview even when playing
                            GUI.enabled = true;
                            if (GUILayout.Button(new GUIContent("■", "Stop"), EditorStyles.miniButton, GUILayout.Width(20f)))
                            {
                                StopPreviewChannel();
                            }
                        }
                        else
                        {
                            GUI.color = new Color(0, 0.545f, 1);
                            // We want to be able to preview even when playing
                            GUI.enabled = true;
                            bool playAudioClip = GUILayout.Button(new GUIContent("►", "Play"), EditorStyles.miniButton, GUILayout.Width(20f));
                            GUI.color = Color.white;
                            if (playAudioClip)
                            {
                                audioManager.playlistFromWhichTheClipIsPreviewed = null;
                                SetPreviewChannel(s, 1, null);
                            }
                        }
                        GUI.enabled = false;
                        EditorGUILayout.ObjectField(s, typeof(AudioClip), false);
                        GUI.enabled = !Application.isPlaying;
                        GUI.color = Color.red;

                        bool removeAudioClip = GUILayout.Button(new GUIContent("X", "Remove"), EditorStyles.miniButton, GUILayout.Width(20f));

                        if (removeAudioClip && EditorUtility.DisplayDialog("Remove this clip ?", string.Format("Are you sure you want to remove the clip '{0}' from AudioManager's list ? It will be removed from your playlists as well !", s.name), "Yes", "No"))
                        {
                            if (audioManager.previewChannel != null && s == audioManager.previewChannel.clip)
                            {
                                // We were playing the sound we want to remove : we have to stop it
                                StopPreviewChannel();
                            }
                            _soundToRemove = s;
                        }

                        GUI.color = Color.white;
                        EditorGUILayout.EndHorizontal();
                    }
                }
                GUI.enabled = wasEnabled;
            }
            else
            {
                // The list is empty, we must clear some variables
                _musicOnStart.stringValue = "";
            }

            UpdateAudioManagerList();

            EditorGUILayout.Space();

            GUI.enabled = true;

            showGeneralParameters = Foldout(showGeneralParameters, "General Parameters", true);

            GUI.enabled = !Application.isPlaying;

            if (showGeneralParameters)
            {
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(_dontDestroyOnLoad);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            if (audioManager.audioClips.Count > 0)
            {
                EditorGUILayout.Space();

                GUI.enabled = true;

                showMusicParameters = Foldout(showMusicParameters, "Music Parameters", true);

                GUI.enabled = !Application.isPlaying;

                if (showMusicParameters)
                {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(_playMusicOnStart, new GUIContent("Play Music On Start", "If checked, the music selected in the dropdown list will play on scene start."));

                    if (_playMusicOnStart.boolValue)
                    {
                        string[] options = new string[audioManager.audioClips.Count];
                        int selectedIndex = 0;
                        for (int i = 0; i < audioManager.audioClips.Count; i++)
                        {
                            options[i] = audioManager.audioClips[i].name;
                            if (_musicOnStart.stringValue == audioManager.audioClips[i].name)
                            {
                                selectedIndex = i;
                            }
                        }
                        _musicOnStart.stringValue = audioManager.audioClips[EditorGUILayout.Popup(selectedIndex, options)].name;
                    }

                    EditorGUILayout.EndHorizontal();

                    if (_playMusicOnStart.boolValue)
                    {
                        EditorGUILayout.PropertyField(_musicVolumeOnStart);
                    }

                    EditorGUILayout.PropertyField(_loopMusicOnStart, new GUIContent("Loop", "If checked, the music channel will loop."));

                    EditorGUILayout.PropertyField(_mustFade, new GUIContent("Must Fade", "If checked, changing music will use adjustable curves to fade in and out."));
                    if (_mustFade.boolValue)
                    {
                        EditorGUILayout.PropertyField(_fadeIn);
                        EditorGUILayout.PropertyField(_fadeOut);
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.Space();

                GUI.enabled = true;

                showSoundParameters = Foldout(showSoundParameters, "Sound Parameters", true);

                GUI.enabled = !Application.isPlaying;

                if (showSoundParameters)
                {
                    EditorGUILayout.BeginVertical("Box");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Max. Sounds at a time", "How many sounds can be played simultaneously ?"));
                    EditorGUILayout.PropertyField(_maxSoundsAtATime, new GUIContent(""));
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();

            GUI.enabled = true;

            showPlaylists = Foldout(showPlaylists, "Playlists", true);

            GUI.enabled = !Application.isPlaying;

            if (GUILayout.Button(new GUIContent("New", "Create a new playlist and give it a name."), EditorStyles.miniButton))
            {
                EditorWindow.GetWindowWithRect<NewPlaylistWindow>(new Rect(Screen.width / 2, Screen.height / 2, 250, 85), true, "New Playlist", true).Init(this);
            }

            if (GUILayout.Button(new GUIContent("Create From Folder", "Choose a folder in your computer and a playlist with the name of the folder will be created. If the folder is not part of your project, you will be asked if you want to import it."), EditorStyles.miniButton))
            {
                string folderPath = EditorUtility.OpenFolderPanel("Create Playlist From Folder", "", "");
                if (!string.IsNullOrEmpty(folderPath))
                {
                    // The folder is not in the Unity project, we ask the user if he wants to import in the Project
                    if (!folderPath.Contains(Application.dataPath))
                    {
                        if (EditorUtility.DisplayDialog("Import folder ?", "This folder is not part of your project.\nDo you want to import it ?\n" +
                                                                             "It will be imported in Assets folder.\n" +
                                                                             "Note : The playlist will not be created automatically, as the folder must be in the Resources folder.", "Yes", "No"))
                        {
                            string folderNameWithSlash = folderPath.Substring(folderPath.LastIndexOf('/'));
                            string folderPathInProject = Application.dataPath + folderNameWithSlash;
                            if (!System.IO.Directory.Exists(folderPathInProject))
                            {
                                System.IO.Directory.CreateDirectory(folderPathInProject);
                            }
                            else
                            {
                                // System.IO.File.Copy and AssetDatabase.ImportAsset handle conflicting files so that's ok if files in selected folder already exist in existing folder
                                if (!EditorUtility.DisplayDialog("Add files to folder ?", "A folder with the name '" + folderNameWithSlash.Substring(1) + "' already exists in Assets folder.\n" +
                                                                                           "Do you still want to add files of the folder you have selected in the existing folder ?\n" +
                                                                                           "If you choose no, the operation will be cancelled.", "Yes", "No"))
                                {
                                    return;
                                }
                            }
                            // Import folder creation in Unity (otherwise we can't the folder in the Project tab)
                            AssetDatabase.ImportAsset("Assets" + folderNameWithSlash);

                            // Get files in selected flolder
                            string[] files = System.IO.Directory.GetFiles(folderPath);
                            // Copy the files and overwrite destination files if they already exist.
                            foreach (string s in files)
                            {
                                string fileName = System.IO.Path.GetFileName(s); // Get only the filename
                                string destFile = System.IO.Path.Combine(folderPathInProject, fileName); // Get the destination path of the file
                                System.IO.File.Copy(s, destFile, true);
                                // Import file in Unity (otherwise we can't see it in the Project tab)
                                AssetDatabase.ImportAsset(destFile.Substring(destFile.LastIndexOf("Assets/")));
                            }
                        }
                    }
                    else
                    {
                        if (!folderPath.Contains("Resources"))
                        {
                            Debug.LogWarning("The folder is not located in the Resources folder. AudioClips could not be loaded.");
                        }
                        else
                        {
                            folderPath = folderPath.Substring(folderPath.IndexOf("Resources") + 9);
                            if (!string.IsNullOrEmpty(folderPath) && folderPath[0] == '/')
                            {
                                // It's not just the Resources folder, it's located in it, so there is a '/' left
                                folderPath = folderPath.Substring(1);
                            }
                            AudioClip[] clips = AddInAudioManagerList(folderPath);
                            // Some clips may have been added to the AudioManager's list, we have to update it to avoid repaint errors
                            UpdateAudioManagerList();
                            folderPath = folderPath.Substring(folderPath.LastIndexOf('/') + 1);
                            // If the folderPath is empty, it was the Resources folder
                            folderPath = string.IsNullOrEmpty(folderPath) ? "Resources" : folderPath;
                            CreateNewPlaylist(folderPath, clips);
                        }
                    }
                }
            }

            if (GUILayout.Button("Remove All", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Remove all playlists ?", "Do you really want to remove all of your playlists ?", "Yes", "No"))
                {
                    audioManager.playlists.Clear();
                    audioManager.playlists = new List<Playlist>();
                    _listControls.Clear();
                    _listControls = new List<ReorderableListControl>();
                    _listAdaptors.Clear();
                    _listAdaptors = new List<IReorderableListAdaptor>();
                    StopPreviewChannel();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                }
            }

            EditorGUILayout.EndHorizontal();

            if (showPlaylists)
            {
                // We iterate our list in reverse so that the RemoveAt works without getting errors
                // Because of that, the playlists are sorted from the most recent to the older one
                for (int i = audioManager.playlists.Count - 1; i >= 0; i--)
                {
                    EditorGUILayout.Space();
                    currentIteratedPlaylist = audioManager.playlists[i];

                    EditorGUILayout.BeginVertical("Box");

                    // Set the title, we save space for foldout option
                    ReorderableListGUI.Title("   " + audioManager.playlists[i].title);

                    Rect invisibleLinePosition = GUILayoutUtility.GetRect(0, 0);

                    // The user must indicate if the playlist is of musics, or sounds (by default sounds)
                    AudioClipType newPlaylistAudioClipType = (AudioClipType)EditorGUI.Popup(new Rect(invisibleLinePosition.x + invisibleLinePosition.width - 187, invisibleLinePosition.y - 17, 60, 20),
                                                                            (int)audioManager.playlists[i].audioClipType, Enum.GetNames(typeof(AudioClipType)), EditorStyles.toolbarPopup);

                    if (newPlaylistAudioClipType != audioManager.playlists[i].audioClipType)
                    {
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                    }
                    audioManager.playlists[i].audioClipType = newPlaylistAudioClipType;

                    if (GUI.Button(new Rect(invisibleLinePosition.x + invisibleLinePosition.width - 127, invisibleLinePosition.y - 18, 64, 20), "Rename", EditorStyles.miniButtonMid))
                    {
                        EditorWindow.GetWindowWithRect<RenamePlaylistWindow>(new Rect(Screen.width / 2, Screen.height / 2, 250, 85), true, "Rename Playlist", true).Init(this, audioManager.playlists[i].title);
                    }

                    if (GUI.Button(new Rect(invisibleLinePosition.x + invisibleLinePosition.width - 65, invisibleLinePosition.y - 18, 64, 20), "Remove", EditorStyles.miniButtonRight))
                    {
                        if (EditorUtility.DisplayDialog("Remove Playlist ?", string.Format("Do you really want to remove the playlist '{0}' ?", audioManager.playlists[i].title), "Yes", "No"))
                        {
                            audioManager.playlists.RemoveAt(i);
                            _listControls.RemoveAt(i);
                            _listAdaptors.RemoveAt(i);
                            if (currentIteratedPlaylist == audioManager.playlistFromWhichTheClipIsPreviewed)
                            {
                                StopPreviewChannel();
                            }
                            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
                            continue;
                        }
                    }

                    // Draw the playlist !

                    // Remove from playlist the audio assets eventually just deleted
                    for (int j = audioManager.playlists[i].sounds.Count - 1; j >= 0; j--)
                    {
                        if (audioManager.playlists[i].sounds[j].clip == null)
                        {
                            audioManager.playlists[i].sounds.RemoveAt(j);
                        }
                    }

                    _listControls[i].Draw(_listAdaptors[i], DrawEmptyPlaylist);

                    audioManager.playlists[i].isFolded = !EditorGUI.Foldout(new Rect(invisibleLinePosition.x + 15, invisibleLinePosition.y - 16, invisibleLinePosition.width, invisibleLinePosition.height), !audioManager.playlists[i].isFolded, "");

                    float newPlaylistVolume = EditorGUILayout.Slider("Volume", audioManager.playlists[i].volume, 0, 1);

                    if (newPlaylistVolume != audioManager.playlists[i].volume)
                    {
                        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving

                        // Update the preview channel volume if we are previewing a clip from this playlist
                        if (currentIteratedPlaylist == audioManager.playlistFromWhichTheClipIsPreviewed)
                        {
                            UpdatePreviewChannelVolume(newPlaylistVolume);
                        }
                    }

                    audioManager.playlists[i].volume = newPlaylistVolume;

                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.Space();
        }

        // Apply changes to all serializedProperties - always do this in the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }

    void UpdateAudioManagerList()
    {
        foreach (AudioClip s in _soundsToAdd)
        {
            audioManager.audioClips.Add(s);
        }
        if (_soundsToAdd.Count > 0)
        {
            // Sort alphabetically
            audioManager.audioClips = audioManager.audioClips.OrderBy(x => x.name).ToList();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
        }
        _soundsToAdd.Clear();

        if (_soundToRemove != null)
        {
            // We remove the clip from all playlists
            foreach (Playlist p in audioManager.playlists)
            {
                for (int i = 0; i < p.sounds.Count; i++)
                {
                    if (p.sounds[i].clip == _soundToRemove)
                    {
                        p.sounds.RemoveAt(i);
                        break;
                    }
                }
            }
            audioManager.audioClips.Remove(_soundToRemove);
            // The AudioManager's list is now empty, we must update the playlists
            if (audioManager.audioClips.Count == 0)
            {
                foreach (ReorderableListControl listControl in _listControls)
                {
                    listControl.AddMenuClicked -= OnAddMenuClicked;
                    listControl.ItemRemoving -= OnItemRemoving;
                    listControl.ItemMoved -= OnItemMoved;
                }
            }
            _soundToRemove = null;
            serializedObject.Update();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); // Force saving
        }
    }

    AudioClip[] AddInAudioManagerList(string folderPath)
    {
        AudioClip[] audioClips = Resources.LoadAll<AudioClip>(folderPath);
        // We add the clips in the AudioManager's list if they are not already
        foreach (AudioClip a in audioClips)
        {
            // Do not add if the clip is already in the list or if two clips have the same name
            if (!audioManager.audioClips.Any(x => x == a || x.name == a.name))
            {
                if (_soundsToAdd.Any(x => x.name == a.name))
                {
                    Debug.LogWarning(string.Format("Two or more clips have the same name '{0}'. Taking the first one.", a.name));
                }
                else
                {
                    _soundsToAdd.Add(a);
                }
            }
        }
        // If it's the first time we add clips to AudioManager, but playlists are already present, we must add listControls
        if (audioManager.audioClips.Count == 0)
        {
            foreach (ReorderableListControl listControl in _listControls)
            {
                listControl.AddMenuClicked += OnAddMenuClicked;
                listControl.ItemRemoving += OnItemRemoving;
                listControl.ItemMoved += OnItemMoved;
            }
        }
        return audioClips;
    }

    void DragAndDropAudioManagerList()
    {
        EditorGUILayout.Space();
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetRect(0.0f, 40.0f, GUILayout.ExpandWidth(true));
        GUIStyle myStyle = GUI.skin.GetStyle("TextArea");
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(1, 1, Color.white);
        tex.Apply();
        myStyle.normal.background = tex;
        myStyle.alignment = TextAnchor.MiddleCenter;
        GUI.Box(drop_area, "Drag'n'Drop here an AudioClip,\n or a folder located in Resources", myStyle);

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:

                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        if (DragAndDrop.objectReferences[i] is AudioClip)
                        {
                            // It's just a single Audioclip
                            // Do not add if the clip is already in the list
                            if (!audioManager.audioClips.Any(x => x == DragAndDrop.objectReferences[i] as AudioClip))
                            {
                                _soundsToAdd.Add(DragAndDrop.objectReferences[i] as AudioClip);
                                // If it's the first time we add clips to AudioManager, but playlists are already present, we must add listControls
                                if (audioManager.audioClips.Count == 0)
                                {
                                    foreach (ReorderableListControl listControl in _listControls)
                                    {
                                        listControl.AddMenuClicked += OnAddMenuClicked;
                                        listControl.ItemRemoving += OnItemRemoving;
                                        listControl.ItemMoved += OnItemMoved;
                                    }
                                }
                            }
                        }
                        else
                        {
                            // It's a folder
                            string folderPath = DragAndDrop.paths[i].Substring(DragAndDrop.paths[i].IndexOf("Resources") + 9);
                            if (!string.IsNullOrEmpty(folderPath) && folderPath[0] == '/')
                            {
                                // It's not just the Resources folder, it's located in it, so there is a '/' left
                                folderPath = folderPath.Substring(1);
                            }
                            // WARNING : If there is a lot of AudioClips to load, the following instruction will take some time
                            // But I haven't found another solution yet since there is no Resources.LoadAllAsync function
                            AddInAudioManagerList(folderPath);
                        }
                    }
                }
                break;
        }
        EditorGUILayout.Space();
    }
}