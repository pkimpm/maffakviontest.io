using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class DialogueView : MonoBehaviour
{
    [Header("UI Компоненты")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerIcon;
    [SerializeField] private Button continueButton;

    [Header("Анимация")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("Защита от быстрого нажатия")]
    [SerializeField] private float clickCooldown = 0.3f;

    public float FadeDuration => fadeDuration;
    public Action OnContinueClick;

    private CanvasGroup canvasGroup;
    private float lastClickTime = 0f;
    private bool isProcessingClick = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
        }
    }

    private void OnContinueButtonClicked()
    {
        float timeSinceLastClick = Time.time - lastClickTime;
        
        if (isProcessingClick || timeSinceLastClick < clickCooldown)
        {
            return;
        }

        if (continueButton != null)
        {
            continueButton.interactable = false;
        }

        isProcessingClick = true;
        lastClickTime = Time.time;
        
        OnContinueClick?.Invoke();

        StartCoroutine(ResetClickProtection());
    }

    private IEnumerator ResetClickProtection()
    {
        yield return new WaitForSeconds(clickCooldown);
        isProcessingClick = false;
    }

    public void SetContent(DialogueNode node)
    {
        if (speakerNameText != null) speakerNameText.text = node.speakerName;
        if (dialogueText != null) dialogueText.text = node.dialogueLine;
        
        if (speakerIcon != null)
        {
            if (node.speakerIcon != null)
            {
                speakerIcon.sprite = node.speakerIcon;
                speakerIcon.enabled = true;
            }
            else
            {
                speakerIcon.enabled = false;
            }
        }
    }

    public void Show()
    {
        if (canvasGroup == null) return;
        
        isProcessingClick = false;
        
        LeanTween.alphaCanvas(canvasGroup, 1f, fadeDuration);
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        if (canvasGroup == null) return;
        
        isProcessingClick = true;
        
        LeanTween.alphaCanvas(canvasGroup, 0f, fadeDuration);
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void SetContinueInteractable(bool isInteractable)
    {
        if (continueButton != null)
        {
            continueButton.interactable = isInteractable;
            
            if (isInteractable)
            {
                isProcessingClick = false;
            }
        }
    }
}