using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class God : MonoBehaviour,
    IEventHandler<NewGodRequestEvent>
{
    [Header("General Settings")]

    [SerializeField]
    [Tooltip("In seconds.")]
    private float showDuration = 3f;

    [Header("Bulle")]

    [SerializeField]
    private GameObject bulleGameObject;
    [SerializeField]
    private Animator bulleAnimator;
    [SerializeField]
    private TMP_Text bulleText;

    [Header("Hand")]

    [SerializeField]
    private GameObject handGameObject;
    [SerializeField]
    private Animator handAnimator;

    [Header("Progress")]
    [SerializeField]
    private Image progressImage;

    private float startShowTime;

    void Start()
    {
        bulleGameObject.SetActive(false);
        handGameObject.SetActive(false);

        this.Subscribe<NewGodRequestEvent>();
    }

    void OnDestroy()
    {
        this.UnSubscribe<NewGodRequestEvent>();
    }

    void IEventHandler<NewGodRequestEvent>.Handle(NewGodRequestEvent @event)
    {
        Show();
        startShowTime = Time.time;
    }

    void Update()
    {
        float t = (Time.time - startShowTime) / showDuration; // between 0 and 1
        progressImage.fillAmount = t;

        if (startShowTime != 0 && t >= 1)
        {
            Hide();
            startShowTime = 0;
        }
    }

    private void Show()
    {
        bulleGameObject.SetActive(true);
        bulleAnimator.SetTrigger("Show");
        bulleText.text = $"J'ai changé d'avis !\nMaintenant, je veux:\n<b>{RulePicker.Instance.NextRule.Text}</b>";

        handGameObject.SetActive(true);
        handAnimator.SetTrigger("Show");
    }

    private void Hide()
    {
        bulleAnimator.SetTrigger("Hide");
        handAnimator.SetTrigger("Hide");
    }
}
