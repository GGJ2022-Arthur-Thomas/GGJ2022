using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayUI : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Color sundayBackgroundColor = Color.gray;

    private readonly string[] dayShortTexts = new[] { "LUN", "MAR", "MER", "JEU", "VEN", "SAM", "DIM" };

    public void SetDay(int dayNb)
    {
        int dayInWeek = dayNb % 7;
        SetText(dayShortTexts[dayInWeek]);
        if (dayInWeek == 6)
        {
            backgroundImage.color = sundayBackgroundColor;
        }
    }

    private void SetText(string text)
    {
        this.text.text = text;
    }
}
