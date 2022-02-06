using UnityEngine;
using UnityEditor;

public class RenamePlaylistWindow : EditorWindow
{
    string playlistPreviousName = "";
    string playlistNewName = "";
    AudioManagerEditor audioManagerEditor;

    public void Init( AudioManagerEditor a, string title )
    {
        audioManagerEditor = a;
        playlistPreviousName = title;
        playlistNewName = title;
    }

    void OnGUI()
    {
        EditorGUILayout.Space();
        GUILayout.Label( "Choose a new name for your playlist : " );
        EditorGUILayout.Space();
        // We automatically focus on the textfield
        GUI.SetNextControlName( "PlaylistName" );
        GUI.FocusControl( "PlaylistName" );
        playlistNewName = GUILayout.TextField( playlistNewName, 50 );
        EditorGUILayout.Space();
        if ( GUILayout.Button( "Validate" ) && !string.IsNullOrEmpty( playlistNewName.Trim() ) )
        {
            audioManagerEditor.RenamePlaylist( playlistPreviousName, playlistNewName );
            Close();
        }

        if ( Event.current.isKey )
        {
            switch ( Event.current.keyCode )
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    if ( !string.IsNullOrEmpty( playlistNewName.Trim() ) )
                    {
                        audioManagerEditor.RenamePlaylist( playlistPreviousName, playlistNewName );
                        Close();
                    }
                    Event.current.Use(); // Consume event to prevent other classes use it
                    break;
            }
        }
    }
}