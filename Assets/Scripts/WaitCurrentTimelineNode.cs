using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Dialogue/WaitCurrentTimelineNode", fileName = "WaitCurrentTimelineNode")]
public class WaitCurrentTimelineNode : DialogueNode
{
    public bool restartFromBeginning = false;
    
    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        var director = manager.GetDirector();
        if (director == null) 
        {
            onExecuteComplete?.Invoke();
            return;
        }

        if (restartFromBeginning)
        {
            director.time = 0;
            director.Evaluate();
        }

        if (director.state != PlayState.Playing)
        {
            director.Play();
        }

        manager.StartCoroutine(WaitForTimeline(director, onExecuteComplete));
    }

    private IEnumerator WaitForTimeline(PlayableDirector director, Action onExecuteComplete)
    {
        yield return new WaitUntil(() => director.state != PlayState.Playing);
        onExecuteComplete?.Invoke();
    }
}