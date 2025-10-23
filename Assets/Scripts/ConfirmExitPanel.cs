using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmExitPanel : MonoBehaviour
{
    [Header("UI компоненты")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Тексты (опционально)")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private string confirmMessage = "Вы уверены, что хотите выйти?";

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.3f;

    public System.Action OnConfirm;
    public System.Action OnCancel;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(() => 
        {
            Hide();
            OnConfirm?.Invoke();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() => 
        {
            Hide();
            OnCancel?.Invoke();
        });

        if (messageText != null)
        {
            messageText.text = confirmMessage;
        }
    }

    public void Show()
    {
        canvasGroup.transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        LeanTween.cancel(canvasGroup.gameObject);
        canvasGroup.alpha = 0f;
        LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setIgnoreTimeScale(true);
    }

    private void Hide()
    {
        LeanTween.cancel(canvasGroup.gameObject);
        LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setIgnoreTimeScale(true)
            .setOnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            });
    }
}