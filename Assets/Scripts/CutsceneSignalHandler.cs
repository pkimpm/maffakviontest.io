using UnityEngine;
using UnityEngine.Playables;

public class CutsceneSignalHandler : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueView dialogueView;
    [SerializeField] private PlayableDirector playableDirector;

    public void OnHalfwaySignal()
    {
        if (playableDirector != null)
        {
            var offsetManager = playableDirector.GetComponent<TimelineOffsetManager>();
            if (offsetManager != null)
            {
                offsetManager.CaptureStateAndPause();
            }
            else
            {
                playableDirector.Pause();
            }
        }

        if (dialogueView != null)
            dialogueView.SetContinueInteractable(true);
    }
}