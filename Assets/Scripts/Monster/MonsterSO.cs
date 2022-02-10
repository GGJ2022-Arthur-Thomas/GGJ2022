using UnityEngine;

[CreateAssetMenu(menuName = "Monster", fileName = "Monster")]
public class MonsterSO : ScriptableObject
{
    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    [TextArea]
    private string description;

    
    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
    public string Description => description;
}
