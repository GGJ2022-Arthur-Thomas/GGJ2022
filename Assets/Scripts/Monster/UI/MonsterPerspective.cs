using System;
using ExtensionMethods;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPerspective : MonoBehaviour
{
    [SerializeField] private Image monsterImage;

    private Transform Transform;

    private float initialScale = 0.0f;
    private float finalScale = 1.0f;
    private float initialLightness = 0.0f;
    private float finalLightness = 1.0f;
    private float timeSinceStart = 1.0f;
    private float duration = 1.0f;

    private void Awake()
    {
        Transform = transform;
    }

    // Update is called once per frame
    private void Update()
    {
        if (timeSinceStart >= duration)
            return;
        
        timeSinceStart = Math.Min(timeSinceStart + Time.deltaTime, duration);
        UpdateUI();
    }

    private void UpdateUI()
    {
        var progress = timeSinceStart / duration;
        var scale = Mathf.Lerp(initialScale, finalScale, progress); 
        monsterImage.transform.SetXScale(scale);
        monsterImage.transform.SetYScale(scale);
        var lightness = Mathf.Lerp(initialLightness, finalLightness, progress); 
        monsterImage.color = new Color(lightness, lightness, lightness);
    }

    public void SetFinalValues(float newScale, float newLightness, float duration)
    {
        if (duration == 0.0f)
        {
            throw new System.ArgumentException("duration can't be zero");
        }
        var progress = timeSinceStart / duration;
        initialScale = Mathf.Lerp(initialScale, finalScale, progress); 
        initialLightness = Mathf.Lerp(initialLightness, finalLightness, progress); 
        finalScale = newScale;
        finalLightness = newLightness;
        timeSinceStart = 0.0f;
        this.duration = duration;
    }

    public void SetInitialValues(float newScale, float newLightness)
    {
        initialScale = newScale; 
        finalScale = newScale;
        initialLightness = newLightness;
        finalLightness = newLightness;
        timeSinceStart = 0.0f;
        duration = 1.0f;
        UpdateUI();
    }
}