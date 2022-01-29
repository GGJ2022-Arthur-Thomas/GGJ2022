using UnityEngine;
using ExtensionMethods;
using UnityEngine.UI;

public class Timeline : MonoBehaviour
{
    [SerializeField] private RectTransform progressBarTransform;
    [SerializeField] private RectTransform fillTransform;
    [SerializeField] private GridLayoutGroup daysGridLayoutGroup;
    [SerializeField] private GameObject dayPrefab;

    [SerializeField]
    [Min(1)]
    private int nbDays = 40;

    [SerializeField]
    [Min(.01f)]
    private float dayDurationInSeconds = 20f;

    private Transform daysGridTransform;

    private float startTime;
    private float totalDuration;
    /// <summary>
    /// Between 0 (start game) and 1 (end game)
    /// </summary>
    private float t;
    private bool ended;
    private float progressBarWidth;

    void Start()
    {
        startTime = Time.time;
        totalDuration = nbDays * dayDurationInSeconds;
        progressBarWidth = GetWidth(progressBarTransform);

        daysGridTransform = daysGridLayoutGroup.transform;
        daysGridLayoutGroup.constraintCount = nbDays;
        daysGridLayoutGroup.cellSize = new Vector2(progressBarWidth / nbDays, daysGridLayoutGroup.cellSize.y);

        SpawnDays();
    }

    private void SpawnDays()
    {
        daysGridTransform.ClearChildren();

        for (int i = 0; i < nbDays; i++)
        {
            var dayGO = Instantiate(dayPrefab, daysGridTransform);
            var dayScript = dayGO.GetComponent<DayUI>();
            dayScript.SetDay(i);
        }
    }

    private void Update()
    {
        if (ended)
            return;

        t = Mathf.Clamp((Time.time - startTime) / totalDuration, 0f, 1f);

        if (t == 1f)
        {
            this.Publish<TimelineEndedEvent>();
            ended = true;
            return;
        }

        UpdateFill();
    }

    private void UpdateFill()
    {
        fillTransform.SetXSize(t * progressBarWidth);
    }

    private float GetWidth(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return corners[2].x - corners[1].x; // topRight.x - topLeft.x
    }
}
