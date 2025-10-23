using UnityEngine;
using System;

[RequireComponent(typeof(PulsingHighlight))]
public class VillainController : MonoBehaviour
{
    [Header("Параметры боя")]
    [SerializeField] private int hitsToDefeat = 5;
    [SerializeField] private float shrinkFactor = 0.9f;
    
    [Header("Эффекты")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private ParticleSystem deathEffect;

    [Header("Анимации урона")]
    [SerializeField] private Animator animatorRig1;
    [SerializeField] private string hitTriggerName1 = "TakeHit";
    [SerializeField] private Animator animatorRig2;
    [SerializeField] private string hitTriggerName2 = "TakeHit";

    [Header("Красное свечение при уроне")]
    [SerializeField] private Material damageMaterial;
    [SerializeField] private float damageFlashDuration = 0.3f;
    [SerializeField] private float damageFlashIntensity = 1.5f;
    [SerializeField] private AnimationCurve damageFlashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private int currentHits;
    private bool canBeHit;
    private PulsingHighlight _highlight;
    private Renderer[] _renderers;
    private Material _damageFlashInstance;
    private LTDescr _damageFlashTween;

    public Action OnDefeated;

    private void Awake()
    {
        _highlight = GetComponent<PulsingHighlight>();
        
        if (damageMaterial != null)
        {
            _damageFlashInstance = new Material(damageMaterial);
            _damageFlashInstance.color = new Color(
                _damageFlashInstance.color.r, 
                _damageFlashInstance.color.g, 
                _damageFlashInstance.color.b, 
                0
            );
            
            var allRenderers = GetComponentsInChildren<Renderer>();
            System.Collections.Generic.List<Renderer> filteredRenderers = new System.Collections.Generic.List<Renderer>();
            foreach (var rend in allRenderers)
            {
                if (rend.GetComponent<ParticleSystem>() == null)
                {
                    filteredRenderers.Add(rend);
                }
            }
            _renderers = filteredRenderers.ToArray();
        }
    }

    public void EnableTargeting()
    {
        canBeHit = true;
        _highlight.StartHighlight();
    }

    public void TakeHit(Vector3 hitPoint)
    {
        if (!canBeHit) return;

        currentHits++;

        if (hitEffect != null)
        {
            hitEffect.transform.position = hitPoint;
            hitEffect.Play();
        }
        
        PlayHitAnimations();
        FlashDamageEffect();

        LeanTween.scale(gameObject, transform.localScale * shrinkFactor, 0.3f)
            .setEase(LeanTweenType.easeInOutQuad);

        if (currentHits >= hitsToDefeat)
        {
            canBeHit = false;
            _highlight.StopHighlight();
            
            OnDefeated?.Invoke();
            
            if (deathEffect != null)
            {
                deathEffect.Play();
                Destroy(gameObject, deathEffect.main.duration);
            }
            else
            {
                Destroy(gameObject, 0.1f);
            }
        }
    }

    private void FlashDamageEffect()
    {
        if (_damageFlashInstance == null || _renderers == null || _renderers.Length == 0) return;

        if (_damageFlashTween != null)
        {
            LeanTween.cancel(_damageFlashTween.id);
        }

        foreach (var rend in _renderers)
        {
            var materials = rend.materials;
            var newMaterials = new Material[materials.Length + 1];
            materials.CopyTo(newMaterials, 0);
            newMaterials[materials.Length] = _damageFlashInstance;
            rend.materials = newMaterials;
        }

        _damageFlashTween = LeanTween.value(gameObject, 0f, 1f, damageFlashDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float t) =>
            {
                float alpha = damageFlashCurve.Evaluate(t) * damageFlashIntensity;
                Color newColor = _damageFlashInstance.color;
                newColor.a = alpha;
                _damageFlashInstance.color = newColor;
            })
            .setOnComplete(() =>
            {
                foreach (var rend in _renderers)
                {
                    var materials = rend.materials;
                    if (materials.Length > 1)
                    {
                        var originalMaterials = new Material[materials.Length - 1];
                        for (int i = 0; i < originalMaterials.Length; i++)
                        {
                            originalMaterials[i] = materials[i];
                        }
                        rend.materials = originalMaterials;
                    }
                }
                _damageFlashTween = null;
            });
    }

    private void PlayHitAnimations()
    {
        if (animatorRig1 != null && !string.IsNullOrEmpty(hitTriggerName1))
            animatorRig1.SetTrigger(hitTriggerName1);
        if (animatorRig2 != null && !string.IsNullOrEmpty(hitTriggerName2))
            animatorRig2.SetTrigger(hitTriggerName2);
    }

    private void OnDestroy()
    {
        if (_damageFlashTween != null)
        {
            LeanTween.cancel(_damageFlashTween.id);
        }
    }
}