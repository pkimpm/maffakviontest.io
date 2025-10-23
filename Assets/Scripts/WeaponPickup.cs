using UnityEngine;

[RequireComponent(typeof(PulsingHighlight))]
public class WeaponPickup : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private VillainController villain;

    [Header("Анимация появления")]
    [SerializeField] private float riseDistance = 0.5f;
    [SerializeField] private float riseDuration = 0.5f;

    [Header("Анимация при нажатии")]
    [SerializeField] private float sinkDistance = 0.3f;
    [SerializeField] private float pickupAnimationDuration = 0.4f;

    private bool isClickable;
    private Vector3 startLocalPos;
    private PulsingHighlight _highlight;

    private void Awake()
    {
        _highlight = GetComponent<PulsingHighlight>();
    }

    private void OnEnable()
    {
        isClickable = false;
        
        startLocalPos = transform.localPosition; 
        Vector3 raisedPos = startLocalPos + new Vector3(0, riseDistance, 0);
        transform.localPosition = startLocalPos;

        LeanTween.moveLocal(gameObject, raisedPos, riseDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() =>
            {
                isClickable = true;
                startLocalPos = transform.localPosition;
                _highlight.StartHighlight();
            });
    }

    public void Pickup()
    {
        if (!isClickable) return;
        isClickable = false;

        _highlight.StopHighlight();

        Vector3 targetPos = startLocalPos - new Vector3(0, sinkDistance, 0);
        
        LeanTween.moveLocal(gameObject, targetPos, pickupAnimationDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                villain?.EnableTargeting();
            });
    }
}