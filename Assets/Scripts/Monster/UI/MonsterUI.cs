using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterUI : MonoBehaviour
{
    private MonsterSO monster;
    private Vector3 initialPosition;
    private Vector3 poi;
    private float timeSinceStart = 1.0f;
    private float duration = 1.0f;

    [SerializeField] private Image monsterImage;

    private void Update()
    {
        if (HasArrived())
            return;

        timeSinceStart = Math.Min(timeSinceStart + Time.deltaTime, duration);
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        transform.position =  new Vector3
        {
            x = Mathf.Lerp(initialPosition.x, poi.x, timeSinceStart / duration),
            y = Mathf.Lerp(initialPosition.y, poi.y, timeSinceStart / duration)
        };
    }

    internal MonsterSO Monster
    {
        get => monster;
        set
        {
            monster = value;
            monsterImage.sprite = value.Sprite;
        }
    }

    internal void SetPOI(Vector3 poi, float duration)
    {
        initialPosition = transform.position;
        this.poi = poi;
        timeSinceStart = 0.0f;
        this.duration = duration;
    }

    public bool HasArrived()
    {
        return timeSinceStart >= duration;
    }
}
