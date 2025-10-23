using UnityEngine;
using UnityEngine.Playables;
using System;
using System.Collections;

[CreateAssetMenu(menuName = "Dialogue/Resume And Wait Timeline Node", fileName = "ResumeAndWaitTimelineNode")]
public class ResumeAndWaitTimelineNode : DialogueNode
{
    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        var director = manager.GetDirector();
        if (director == null)
        {
            onExecuteComplete?.Invoke();
            return;
        }

        var offsetManager = director.GetComponent<TimelineOffsetManager>();
        
        if (director.state == PlayState.Paused)
        {
            if (offsetManager != null)
            {
                offsetManager.ResumeWithOffset();
            }
            else
            {
                director.Resume();
            }
        }
        else if (director.state != PlayState.Playing)
        {
            director.Play();
        }
        
        manager.StartCoroutine(WaitForTimeline(director, onExecuteComplete));
    }

    private IEnumerator WaitForTimeline(PlayableDirector director, Action onExecuteComplete)
    {
        yield return null;
        yield return new WaitUntil(() => director.state != PlayState.Playing);
        onExecuteComplete?.Invoke();
    }
}