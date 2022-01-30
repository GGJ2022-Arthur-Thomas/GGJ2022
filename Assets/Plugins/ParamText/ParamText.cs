using UnityEngine;
using UnityEngine.UI;

public abstract class ParamText<T> : MonoBehaviour where T : MaskableGraphic
{

    [SerializeField]
    protected T textComponent = null;

    [SerializeField]
    [Tooltip("If empty, will be initialized with the text already set on the component, on the first call to SetText.")]
    private string baseString = "";


    public T Text => textComponent;


    protected abstract string GetText();
    protected abstract void SetText(string text);


    public void SetText(params object[] args)
    {
        TryInitBaseString();

        SetText(baseString);

        for (var i = 0; i < args.Length; i++)
        {
            SetText(GetText().Replace($"{{{i}}}", args[i].ToString()));
        }
    }

    /// <summary>
    /// Just calls <see cref="SetAbsoluteText(string)"/> with an empty string as parameter.
    /// </summary>
    public void CleanText()
    {
        SetAbsoluteText("");
    }

    /// <summary>
    /// Use this if you want to set the overall string, and not just its arguments.
    /// </summary>
    public void SetAbsoluteText(string text)
    {
        SetText(text);
    }

    private void TryInitBaseString()
    {
        if (string.IsNullOrEmpty(baseString))
        {
            baseString = GetText();
        }
    }
}