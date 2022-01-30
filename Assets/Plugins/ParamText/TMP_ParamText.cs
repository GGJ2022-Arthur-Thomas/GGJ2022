using TMPro;

public class TMP_ParamText : ParamText<TMP_Text>
{
    protected override string GetText() => textComponent.text;

    protected override void SetText(string text)
    {
        textComponent.text = text;
    }
}