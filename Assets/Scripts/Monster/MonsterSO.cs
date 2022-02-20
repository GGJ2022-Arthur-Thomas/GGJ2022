using UnityEngine;

[CreateAssetMenu(menuName = "Monster", fileName = "Monster")]
public class MonsterSO : ScriptableObject
{
    [SerializeField]
    private string displayName;

    [SerializeField]
    private Sprite sprite;

    [SerializeField]
    [Min(0.01f)]
    private float size = 1f;

    [SerializeField]
    [TextArea]
    private string description;

    
    public string DisplayName => displayName;
    public Sprite Sprite => sprite;
    public float Size => size;
    public string Description => description;
}
