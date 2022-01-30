using ExtensionMethods;
using UnityEngine;

public class LifeCountUI : MonoBehaviour,
    IEventHandler<LifeCountChangedEvent>
{
    [SerializeField] private GameObject lifePrefab;
    [SerializeField] private Transform lifeParent;

    private void Start()
    {
        SpawnLives();
        this.Subscribe<LifeCountChangedEvent>();
    }

    private void SpawnLives()
    {
        lifeParent.ClearChildren();

        for (int i = 0; i < ScoreManager.Lives; i++)
        {
            var lifeGO = Instantiate(lifePrefab, lifeParent);
            lifeGO.name = $"Life_{i}";
        }
    }

    private void OnDestroy()
    {
        this.UnSubscribe<LifeCountChangedEvent>();
    }

    void IEventHandler<LifeCountChangedEvent>.Handle(LifeCountChangedEvent @event)
    {
        Destroy(lifeParent.GetChild(lifeParent.childCount - 1).gameObject);
    }
}
