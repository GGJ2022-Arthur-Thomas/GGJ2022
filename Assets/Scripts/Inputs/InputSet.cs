using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "InputSet", fileName = "InputSet")]
public class InputSet : ScriptableObject
{
    [SerializeField] private bool isAccepted;
    [SerializeField] private KeyCode[] keys;

    public bool IsAccepted => isAccepted;
    
    public bool Contains(KeyCode key)
    {
        return keys.Contains(key);
    }
}
