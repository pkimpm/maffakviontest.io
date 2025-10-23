using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PuzzlePiece : MonoBehaviour
{
    [Header("Правильный слот для этого куска")]
    [SerializeField] private PuzzleSlot correctSlot;

    [Header("Подсказка для этого куска")]
    [SerializeField] private HintData pieceHint;

    [Header("Ограничитель движения")]
    [SerializeField] private Transform moveContainer;

    [Header("Преобразования при перетаскивании")]
    [SerializeField] private Vector3 targetLocalScale = Vector3.one * 1.2f;
    [SerializeField] private Vector3 targetLocalEulerAngles = Vector3.zero;
    [SerializeField] private float transformLerpSpeed = 10f;
    [SerializeField] private float returnDuration = 0.25f;
    [SerializeField] private bool resetTransformsOnPlace = false;

    [Header("Эффекты на куске")]
    [SerializeField] private ParticleSystem pieceVFX1;
    [SerializeField] private ParticleSystem pieceVFX2;
    
    [Header("Время для промотки VFX")]
    [SerializeField, Tooltip("Время для промотки VFX (в секундах)")]
    private float vfxSimulateTime = 600.0f;

    [HideInInspector] public PuzzleManager Manager;
    public HintData PieceHint => pieceHint;

    private Camera mainCamera;
    private bool isDragging;
    private bool isPlacedCorrectly;
    private Vector3 initialPosition;
    private Vector3 initialLocalScale;
    private Quaternion initialLocalRotation;
    private PuzzleSlot currentSlot;
    private Coroutine smoothTransformCoroutine;
    private Coroutine returnCoroutine;

    public bool IsBusy => returnCoroutine != null;

    private void Awake()
    {
        mainCamera = Camera.main;
        initialPosition = transform.position;
        initialLocalScale = transform.localScale;
        initialLocalRotation = transform.localRotation;
    }
    
    public void OnGrab()
    {
        if (Manager == null || !Manager.IsInteractable || isPlacedCorrectly)
        {
            if (isPlacedCorrectly && pieceHint != null)
                HintManager.Instance.ShowHint(pieceHint, HintManager.HintButtonType.Close);
            return;
        }

        isDragging = true;

        StopVFX(pieceVFX1);
        StopVFX(pieceVFX2);

        StartSmoothTransform();
    }
    
    public void OnRelease()
    {
        if (!isDragging) return;

        isDragging = false;
        StopSmoothTransform();
        TrySnapToAnySlot();
    }

    public void DragFollowMouse()
    {
        if (!isDragging) return;

        Vector3 mp = Input.mousePosition;
        mp.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mp);
        transform.position = ClampToContainer(worldPos);
    }
    
    private void TrySnapToAnySlot()
    {
        PuzzleSlot nearest = null;
        float minDist = float.MaxValue;

        foreach (var slot in FindObjectsOfType<PuzzleSlot>())
        {
            if (slot.Occupied && slot != currentSlot) continue;

            float dist = Vector3.Distance(transform.position, slot.transform.position);
            if (dist < minDist && dist <= slot.MagnetDistance)
            {
                minDist = dist;
                nearest = slot;
            }
        }

        if (nearest != null)
        {
            SnapToSlot(nearest);
        }
        else
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnToInitial());
        }
    }

    private void SnapToSlot(PuzzleSlot slot)
    {
        if (slot.Occupied && slot != currentSlot)
        {
            if (returnCoroutine != null) StopCoroutine(returnCoroutine);
            returnCoroutine = StartCoroutine(ReturnToInitial());
            return;
        }

        if (currentSlot != null && currentSlot != slot)
            currentSlot.Free();

        transform.position = slot.transform.position;
        transform.rotation = slot.transform.rotation;

        if (resetTransformsOnPlace)
        {
            transform.localScale = initialLocalScale;
            transform.localRotation = initialLocalRotation;
        }

        bool correct = (slot == correctSlot);
        slot.Occupy(correct);

        currentSlot = slot;
        isPlacedCorrectly = correct;

        if (isPlacedCorrectly)
        {
            Manager?.OnPiecePlaced(this);
            ControlVFXByName(gameObject, "VFX", false);
            slot.ControlChildVFX("VFX", false);
        }
    }

    private IEnumerator ReturnToInitial()
    {
        if (currentSlot != null)
        {
            currentSlot.Free();
            currentSlot = null;
        }

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 startScale = transform.localScale;
        Quaternion startRot = transform.localRotation;

        while (elapsed < returnDuration)
        {
            float t = elapsed / returnDuration;
            transform.position = Vector3.Lerp(startPos, initialPosition, t);
            transform.localScale = Vector3.Lerp(startScale, initialLocalScale, t);
            transform.localRotation = Quaternion.Slerp(startRot, initialLocalRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = initialPosition;
        transform.localScale = initialLocalScale;
        transform.localRotation = initialLocalRotation;

        RestartVFXWithTime(pieceVFX1, vfxSimulateTime);
        RestartVFXWithTime(pieceVFX2, vfxSimulateTime);

        returnCoroutine = null;
    }

    private void StartSmoothTransform()
    {
        if (smoothTransformCoroutine != null) StopCoroutine(smoothTransformCoroutine);
        smoothTransformCoroutine = StartCoroutine(SmoothTransformWhileDragging());
    }

    private void StopSmoothTransform()
    {
        if (smoothTransformCoroutine != null)
        {
            StopCoroutine(smoothTransformCoroutine);
            smoothTransformCoroutine = null;
        }
    }

    private IEnumerator SmoothTransformWhileDragging()
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transformLerpSpeed;
            transform.localScale = Vector3.Lerp(transform.localScale, targetLocalScale, t);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(targetLocalEulerAngles), t);
            yield return null;
        }

        RestartVFXWithTime(pieceVFX1, vfxSimulateTime);
        RestartVFXWithTime(pieceVFX2, vfxSimulateTime);

        while (isDragging)
        {
            transform.localScale = targetLocalScale;
            transform.localRotation = Quaternion.Euler(targetLocalEulerAngles);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transformLerpSpeed;
            transform.localScale = Vector3.Lerp(transform.localScale, initialLocalScale, t);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, initialLocalRotation, t);
            yield return null;
        }
        
        smoothTransformCoroutine = null;
    }
    
    private void RestartVFXWithTime(ParticleSystem vfx, float time)
    {
        if (vfx == null) return;
        
        vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        vfx.Simulate(time, true, true);
        vfx.Play();
    }
    
    private void StopVFX(ParticleSystem vfx)
    {
        if (vfx != null) 
            vfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
    
    private void ControlVFXByName(GameObject parent, string name, bool active)
    {
        foreach (var ps in parent.GetComponentsInChildren<ParticleSystem>())
        {
            if (ps.gameObject.name == name)
            {
                if (active) ps.Play();
                else ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
    
    private Vector3 ClampToContainer(Vector3 worldPos)
    {
        if (moveContainer == null) return worldPos;
        Vector3 local = moveContainer.InverseTransformPoint(worldPos);
        if (moveContainer.TryGetComponent<Collider>(out var col))
        {
            var ext = col.bounds.extents;
            local.x = Mathf.Clamp(local.x, -ext.x, ext.x);
            local.y = Mathf.Clamp(local.y, -ext.y, ext.y);
            local.z = 0f;
        }
        return moveContainer.TransformPoint(local);
    }
}