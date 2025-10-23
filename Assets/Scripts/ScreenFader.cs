using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    [Header("Длительность фейда по умолчанию")]
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup _canvasGroup;
    private LTDescr _tween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeOut(float duration = -1f)
    {
        transform.SetAsLastSibling();
        if (duration < 0f) duration = fadeDuration;
        _canvasGroup.blocksRaycasts = true;

        if (_tween != null) LeanTween.cancel(_tween.id);

        _tween = LeanTween.alphaCanvas(_canvasGroup, 1f, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() => _tween = null);

        while (_tween != null && LeanTween.isTweening(_tween.id))
            yield return null;
    }

    public IEnumerator FadeIn(float duration = -1f)
    {
        transform.SetAsLastSibling();
        if (duration < 0f) duration = fadeDuration;

        if (_tween != null) LeanTween.cancel(_tween.id);

        _tween = LeanTween.alphaCanvas(_canvasGroup, 0f, duration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                _canvasGroup.blocksRaycasts = false;
                _tween = null;
            });

        while (_tween != null && LeanTween.isTweening(_tween.id))
            yield return null;
    }
}