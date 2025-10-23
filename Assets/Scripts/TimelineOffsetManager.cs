using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineOffsetManager : MonoBehaviour
{
    [Header("Объект, который анимирует Timeline")]
    [Tooltip("Перетащите сюда объект персонажа, чье положение анимируется")]
    [SerializeField] private Transform animatedCharacter;

    private PlayableDirector director;
    private double pausedTime;
    private Vector3 pausedPosition;
    private Quaternion pausedRotation;
    private bool isPaused = false;
    private bool shouldBePaused = false;

    private void Awake()
    {
        director = GetComponent<PlayableDirector>();
        if (animatedCharacter == null)
        {
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (shouldBePaused && !isPaused)
        {
            pausedTime = director.time;
            pausedPosition = animatedCharacter.position;
            pausedRotation = animatedCharacter.rotation;
            isPaused = true;
            director.Pause();
        }

        if (isPaused)
        {
            director.time = pausedTime;
            director.Evaluate();
            animatedCharacter.position = pausedPosition;
            animatedCharacter.rotation = pausedRotation;
        }
    }

    public void CaptureStateAndPause()
    {
        if (isPaused || director.state != PlayState.Playing) return;
        shouldBePaused = true;
    }

    public void ResumeWithOffset()
    {
        if (!isPaused)
        {
            if (director.state != PlayState.Playing)
            {
                director.Play();
            }
            return;
        }

        shouldBePaused = false;
        isPaused = false;
        
        director.time = pausedTime;
        director.Resume();
    }
}