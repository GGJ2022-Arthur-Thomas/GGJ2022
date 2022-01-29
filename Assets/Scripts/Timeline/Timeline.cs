using UnityEngine;
using ExtensionMethods;
using UnityEngine.UI;

public class Timeline : Singleton<Timeline>
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

    private bool ended;
    private float totalDuration;
    private float progressBarWidth;

    /// <summary>
    /// Between 0 and <see cref="nbDays"/>.
    /// </summary>
    private int currentDayNb;
    /// <summary>
    /// Between 0 (start of day) and <see cref="dayDurationInSeconds"/>.
    /// </summary>
    private float currentDayTime;

    /// <summary>
    /// Between 0 and <see cref="totalDuration"/>.
    /// </summary>
    private float CurrentTime => currentDayNb * dayDurationInSeconds + currentDayTime;

    /// <summary>
    /// Between 0 (start game) and 1 (end game).
    /// </summary>
    private float T => CurrentTime / totalDuration;

    void Start()
    {
        ended = false;
        totalDuration = nbDays * dayDurationInSeconds;
        progressBarWidth = GetWidth(progressBarTransform);

        currentDayNb = 0;
        currentDayTime = 0f;

        daysGridTransform = daysGridLayoutGroup.transform;
        daysGridLayoutGroup.constraintCount = nbDays;
        daysGridLayoutGroup.cellSize = new Vector2(progressBarWidth / nbDays, daysGridLayoutGroup.cellSize.y);

        SpawnDays();

        NotifyNewDay();
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

        currentDayTime += Time.deltaTime;

        if (currentDayTime >= dayDurationInSeconds)
        {
            currentDayTime = 0f;
            currentDayNb++;

            if (currentDayNb == nbDays)
            {
                this.Publish<TimelineEndedEvent>();
                ended = true;
                return;
            }
            else
            {
                NotifyNewDay();
            }
        }

        UpdateFill();
    }

    private void NotifyNewDay()
    {
        this.Publish(new NewDayEvent(currentDayNb));
    }

    private void UpdateFill()
    {
        fillTransform.SetXSize(T * progressBarWidth);
    }

    private float GetWidth(RectTransform rectTransform)
    {
        var corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        return corners[2].x - corners[1].x; // topRight.x - topLeft.x
    }
}
