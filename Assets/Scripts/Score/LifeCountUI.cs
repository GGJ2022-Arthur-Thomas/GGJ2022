using ExtensionMethods;
using UnityEngine;

public class LifeCountUI : MonoBehaviour,
    IEventHandler<PlayerLostLifeEvent>
{
    [SerializeField] private GameObject lifePrefab;
    [SerializeField] private Transform lifeParent;

    private void Start()
    {
        SpawnLives();
        this.Subscribe<PlayerLostLifeEvent>();
    }

    private void SpawnLives()
    {
        lifeParent.ClearChildren();

        for (int i = 0; i < GameData.Lives; i++)
        {
            var lifeGO = Instantiate(lifePrefab, lifeParent);
            lifeGO.name = $"Life_{i}";
        }
    }

    private void OnDestroy()
    {
        this.UnSubscribe<PlayerLostLifeEvent>();
    }

    void IEventHandler<PlayerLostLifeEvent>.Handle(PlayerLostLifeEvent @event)
    {
        Destroy(lifeParent.GetChild(lifeParent.childCount - 1).gameObject);
    }
}
