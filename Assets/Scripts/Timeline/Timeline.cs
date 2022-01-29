using UnityEngine;
using ExtensionMethods;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class Timeline : Singleton<Timeline>
{
    [Header("Progress bar")]

    [SerializeField] private RectTransform progressBarTransform;
    [SerializeField] private RectTransform fillTransform;

    [Header("Days grid")]

    [SerializeField] private GridLayoutGroup daysGridLayoutGroup;
    [SerializeField] private GameObject dayPrefab;

    [Header("God requests")]

    [SerializeField] private GameObject godRequestPrefab;
    [SerializeField] private Transform godRequestsParent;
    [SerializeField]
    [Min(0)]
    private int minGodRequests;

    [SerializeField]
    [Min(0)]
    private int maxGodRequests;

    [SerializeField]
    [Min(0)]
    [Tooltip("In seconds.")]
    private int minTimeBetweenGodRequests = 30;

    [Header("Day settings")]
    
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
    private float[] godRequestTimes;
    private int nextGodRequestIndex;
    private float NextGodRequestTime => godRequestTimes[nextGodRequestIndex];
    private bool HasNextGodRequest => nextGodRequestIndex < godRequestTimes.Length;

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
        godRequestTimes = CalculateGodRequestTimes();
        nextGodRequestIndex = 0;
        SpawnGodRequests();

        NotifyNewDay();
    }

    private void SpawnDays()
    {
        daysGridTransform.ClearChildren();

        for (int i = 0; i < nbDays; i++)
        {
            var dayGO = Instantiate(dayPrefab, daysGridTransform);
            dayGO.name = $"Day_{i}";
            var dayScript = dayGO.GetComponent<DayUI>();
            dayScript.SetDay(i);
        }
    }

    private void SpawnGodRequests()
    {
        godRequestsParent.ClearChildren();

        for (int i = 0; i < godRequestTimes.Length; i++)
        {
            var godRequestGO = Instantiate(godRequestPrefab, godRequestsParent);
            godRequestGO.name = $"GodRequest_{i}";
            godRequestGO.GetComponent<Image>().enabled = true; // for a weird reason, Image component gets disabled when spawning
            godRequestGO.transform.Translate(Vector3.right * ((godRequestTimes[i] / totalDuration) * progressBarWidth - progressBarWidth / 2));
        }
    }

    private float[] CalculateGodRequestTimes()
    {
        int nbGodRequests = Random.Range(minGodRequests, maxGodRequests);
        var tmpList = new List<(float, bool)>(); // = (time, takeIntoFinalResult)

        for (int i = 0; i < nbGodRequests; i++)
        {
            tmpList.Add((Random.Range(minTimeBetweenGodRequests, totalDuration), true));
        }

        tmpList = tmpList.OrderBy(x => x.Item1).ToList(); // sort by time

        for (int i = 1; i < nbGodRequests; i++)
        {
            if (tmpList[i].Item1 - tmpList[i - 1].Item1 < minTimeBetweenGodRequests)
            {
                tmpList[i] = (tmpList[i].Item1, false); // too close to previous, don't take into final result
            }
        }

        // don't take into final result on sundays (god is resting)
        tmpList = tmpList.Where(x => GetDayInWeekNb(x.Item1) != 6).ToList();

        return tmpList.Where(x => x.Item2 == true).Select(x => x.Item1).ToArray();
    }

    /// <summary>
    /// NOTE: day in week nb is [0-6]
    /// </summary>
    private int GetDayInWeekNb(float time)
    {
        return GetDayNb(time) % 7;
    }

    /// <summary>
    /// NOTE : day nb is [0-nbDays]
    /// </summary>
    private int GetDayNb(float time)
    {
        return (int)((time / totalDuration) * nbDays);
    }

    void Update()
    {
        if (ended)
            return;

        float previousTime = CurrentTime;
        currentDayTime += Time.deltaTime;

        ////////////////////////////////// DEBUG: TO REMOVE
        if (Input.GetKey(KeyCode.Keypad0))
            currentDayTime += 20 * Time.deltaTime;

        if (HasNextGodRequest &&
            previousTime <= NextGodRequestTime && CurrentTime > NextGodRequestTime) // just passed a new god request
        {
            NotifyNewGodRequest();
            nextGodRequestIndex++;
        }

        if (currentDayTime >= dayDurationInSeconds) // just passed a new day
        {
            currentDayTime = 0f;
            currentDayNb++;

            if (currentDayNb < nbDays) // regular new day
            {
                NotifyNewDay();
            }
            else // end of last day
            {
                this.Publish<TimelineEndedEvent>();
                ended = true;
                return;
            }
        }

        UpdateFill();
    }

    private void NotifyNewGodRequest()
    {
        this.Publish<NewGodRequestEvent>();
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
