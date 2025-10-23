using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemReceivedPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button continueButton;

    public System.Action OnContinue;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() =>
        {
            Hide();
            OnContinue?.Invoke();
        });
    }

    public void Show(string message)
    {
        messageText.text = message;
        LeanTween.cancel(canvasGroup.gameObject);
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        LeanTween.alphaCanvas(canvasGroup, 1f, 0.3f).setEase(LeanTweenType.easeInOutQuad);
    }

    private void Hide()
    {
        LeanTween.alphaCanvas(canvasGroup, 0f, 0.3f).setOnComplete(() =>
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });
    }
}