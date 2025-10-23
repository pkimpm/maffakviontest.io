using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TimelineAnimatorBridge : MonoBehaviour
{
    private Animator animator;

    // Хэшируем имена параметров для производительности
    private static readonly int IdleStateTrigger = Animator.StringToHash("GoToIdle");

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Этот метод будет вызываться сигналом из главного таймлайна
    public void TransitionToIdle()
    {
        if (animator != null)
        {
            // Предполагается, что в вашем Animator Controller есть триггер "GoToIdle",
            // который осуществляет переход в состояние Idle из любого другого состояния.
            animator.SetTrigger(IdleStateTrigger);
        }
    }
}