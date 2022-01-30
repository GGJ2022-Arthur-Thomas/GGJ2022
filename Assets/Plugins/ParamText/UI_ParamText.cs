using UnityEngine.UI;

public class UI_ParamText : ParamText<Text>
{
    protected override string GetText()
    {
        return textComponent.text;
    }

    protected override void SetText(string text)
    {
        textComponent.text = text;
    }
}