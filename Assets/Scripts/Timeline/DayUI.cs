using TMPro;
using UnityEngine;

public class DayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text text;

    //private readonly string[] dayTexts = new[] { "Lundi", "Mardi", "Mercredi", "Jeudi", "Vendredi", "Samedi", "Dimanche" };
    private readonly string[] dayShortTexts = new[] { "LUN", "MAR", "MER", "JEU", "VEN", "SAM", "DIM" };

    public void SetDay(int dayNb)
    {
        SetText(dayShortTexts[dayNb % 7]);
    }

    private void SetText(string text)
    {
        this.text.text = text;
    }
}
