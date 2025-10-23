using System.Collections;
using UnityEngine;

public class EnvironmentTrigger : MonoBehaviour
{
    [Header("Основные настройки")]
    [Tooltip("Включить смену окружения и затемнение экрана")]
    [SerializeField] private bool changeEnvironment = true;

    [Tooltip("Камера, которая активирует триггер")]
    [SerializeField] private Camera targetCamera;

    [Tooltip("Контроллер окружения для смены состояния")]
    [SerializeField] private EnvironmentController environmentController;

    [Tooltip("Индекс состояния окружения для переключения")]
    [SerializeField] private int targetStateIndex = 0;

    [Header("Фейд экрана")]
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private float fadeOutDuration = 1f;
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private float fadeInDuration = 1f;

    [Header("Объекты для управления (выполняется всегда)")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;
    [SerializeField] private GameObject[] objectsToDestroy;

    [Header("Опции")]
    [Tooltip("Сработать только один раз")]
    [SerializeField] private bool triggerOnce = true;

    [Tooltip("Удалить триггер после срабатывания")]
    [SerializeField] private bool selfDestruct = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && triggerOnce) return;

        if (targetCamera != null && other.gameObject == targetCamera.gameObject)
        {
            StartCoroutine(TriggerSequence());
            hasTriggered = true;
        }
    }

    private IEnumerator TriggerSequence()
    {
        ExecuteObjectActions();

        if (!changeEnvironment)
        {
            if (selfDestruct) Destroy(gameObject);
            yield break;
        }

        if (screenFader != null)
            yield return screenFader.FadeOut(fadeOutDuration);

        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        environmentController?.SetState(targetStateIndex);

        DynamicGI.UpdateEnvironment();

        if (screenFader != null)
            yield return screenFader.FadeIn(fadeInDuration);

        if (selfDestruct)
            Destroy(gameObject);
    }

    private void ExecuteObjectActions()
    {
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);

        foreach (var obj in objectsToDestroy)
            if (obj != null && obj.scene.IsValid()) Destroy(obj);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
