using UnityEngine;
using UnityEditor;

public class NewPlaylistWindow : EditorWindow
{
    string playlistName = "";
    AudioManagerEditor audioManagerEditor;

    public void Init( AudioManagerEditor a )
    {
        audioManagerEditor = a;
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label( "Choose a new name for your playlist : " );
        EditorGUILayout.Space();
        // We automatically focus on the textfield
        GUI.SetNextControlName( "PlaylistName" );
        GUI.FocusControl( "PlaylistName" );
        playlistName = GUILayout.TextField( playlistName, 50 );
        EditorGUILayout.Space();
        if ( GUILayout.Button( "Validate" ) && !string.IsNullOrEmpty( playlistName.Trim() ) )
        {
            audioManagerEditor.CreateNewPlaylist( playlistName );
            Close();
        }

        if ( Event.current.isKey )
        {
            switch ( Event.current.keyCode )
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if ( !string.IsNullOrEmpty( playlistName.Trim() ) )
                    {
                        audioManagerEditor.CreateNewPlaylist( playlistName );
                        Close();
                    }
                    Event.current.Use(); // Consume event to prevent other classes use it
                    break;
            }
        }
    }
}