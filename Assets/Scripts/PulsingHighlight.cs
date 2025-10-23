using UnityEngine;
using System.Collections.Generic;

public class PulsingHighlight : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private float maxAlpha = 0.6f;
    [SerializeField] private float pulseDuration = 2.5f;
    [SerializeField] private LeanTweenType pulseEaseType = LeanTweenType.easeInOutSine;

    private Renderer[] _renderers;
    private Dictionary<Renderer, Material[]> _originalMaterialsMap;
    private Dictionary<Renderer, Material[]> _highlightedMaterialsMap;
    private Material _highlightInstance;
    private LTDescr _pulseTween;
    private bool isHighlightActive = false;

    private void Awake()
    {
        var allRenderers = GetComponentsInChildren<Renderer>();
        
        List<Renderer> filteredRenderers = new List<Renderer>();
        foreach (var rend in allRenderers)
        {
            if (rend.GetComponent<ParticleSystem>() == null)
            {
                filteredRenderers.Add(rend);
            }
        }
        
        _renderers = filteredRenderers.ToArray();

        _originalMaterialsMap = new Dictionary<Renderer, Material[]>();
        _highlightedMaterialsMap = new Dictionary<Renderer, Material[]>();

        _highlightInstance = new Material(highlightMaterial);
        _highlightInstance.color = new Color(_highlightInstance.color.r, _highlightInstance.color.g, _highlightInstance.color.b, 0);

        foreach (var rend in _renderers)
        {
            var originalMaterials = rend.materials;
            _originalMaterialsMap[rend] = originalMaterials;

            var newMaterials = new Material[originalMaterials.Length + 1];
            originalMaterials.CopyTo(newMaterials, 0);
            newMaterials[originalMaterials.Length] = _highlightInstance;
            _highlightedMaterialsMap[rend] = newMaterials;
        }
    }

    public void StartHighlight()
    {
        if (isHighlightActive || _pulseTween != null) return;
        isHighlightActive = true;

        foreach (var rend in _renderers)
        {
            rend.materials = _highlightedMaterialsMap[rend];
        }

        _pulseTween = LeanTween.value(gameObject, 0f, maxAlpha, pulseDuration)
            .setLoopPingPong()
            .setEase(pulseEaseType)
            .setOnUpdate((float alphaValue) =>
            {
                Color newColor = _highlightInstance.color;
                newColor.a = alphaValue;
                _highlightInstance.color = newColor;
            });
    }

    public void StopHighlight()
    {
        if (!isHighlightActive) return;
        isHighlightActive = false;

        if (_pulseTween != null)
        {
            LeanTween.cancel(_pulseTween.id);
            _pulseTween = null;
        }
        
        float startAlpha = _highlightInstance.color.a;
        LeanTween.value(gameObject, startAlpha, 0f, 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float alphaValue) =>
            {
                Color newColor = _highlightInstance.color;
                newColor.a = alphaValue;
                _highlightInstance.color = newColor;
            })
            .setOnComplete(() =>
            {
                foreach (var rend in _renderers)
                {
                    rend.materials = _originalMaterialsMap[rend];
                }
            });
    }
    
    private void OnDestroy()
    {
        if (_pulseTween != null)
        {
            LeanTween.cancel(_pulseTween.id);
        }
    }
}