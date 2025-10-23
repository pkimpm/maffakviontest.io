using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class LoadingScreen : MonoBehaviour
{
    [Header("UI-компоненты")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private Image fillImage;

    [Header("Плавное затухание")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField, Tooltip("Длительность плавного проявления сцены после загрузки")]
    private float fadeInDuration = 1f;
    
    [SerializeField, Tooltip("Задержка перед затуханием ScreenFader для стабилизации освещения")]
    private float delayBeforeFade = 0.5f;

    [Header("Настройки")]
    [SerializeField, Tooltip("Удалить объект после завершения")]
    private bool destroyAfterUse = true;
    
    [SerializeField, Tooltip("Длительность загрузки")]
    private float loadingDuration = 2f;

    public event Action OnLoadingFinished;

    private bool hasFinished = false;

    private void Awake()
    {
        if (screenFader == null)
        {
            screenFader = FindObjectOfType<ScreenFader>();
        }
        
        if (screenFader != null)
        {
            var cg = screenFader.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 1f;
                cg.blocksRaycasts = true;
            }
        }
    }

    private void OnEnable()
    {
        if (!hasFinished)
        {
            StartLoading();
        }
    }

    private void OnDisable()
    {
        if (canvasGroup != null)
            LeanTween.cancel(canvasGroup.gameObject);
    }

    private void StartLoading()
    {
        if (canvasGroup == null)
        {
            OnLoadingFinished?.Invoke();
            return;
        }

        if (progressBar != null)
            progressBar.value = 0f;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        SimulateLoading();
    }

    private void SimulateLoading()
    {
        LeanTween.value(gameObject, 0f, 1f, loadingDuration)
            .setEase(LeanTweenType.linear)
            .setOnUpdate((float v) =>
            {
                if (progressBar != null)
                    progressBar.value = v;
            })
            .setOnComplete(() =>
            {
                if (progressBar != null)
                    progressBar.value = 1f;

                StartCoroutine(HideLoadingScreen());
            });
    }

    private IEnumerator HideLoadingScreen()
    {
        hasFinished = true;
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        OnLoadingFinished?.Invoke();

        yield return new WaitForSeconds(delayBeforeFade);

        if (screenFader != null)
        {
            yield return screenFader.FadeIn(fadeInDuration);
        }

        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ForceComplete()
    {
        LeanTween.cancel(gameObject);

        hasFinished = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        OnLoadingFinished?.Invoke();

        if (destroyAfterUse)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}