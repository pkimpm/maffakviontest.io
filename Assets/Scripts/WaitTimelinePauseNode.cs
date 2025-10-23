using UnityEngine;
using UnityEngine.Playables;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Dialogue/Wait Timeline Pause Node", fileName = "WaitTimelinePauseNode")]
public class WaitTimelinePauseNode : DialogueNode
{
    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        var director = manager.GetDirector();
        if (director == null || director.state == PlayState.Paused)
        {
            onExecuteComplete?.Invoke();
            return;
        }

        manager.StartCoroutine(WaitForPause(director, onExecuteComplete));
    }

    private IEnumerator WaitForPause(PlayableDirector director, Action onExecuteComplete)
    {
        yield return new WaitUntil(() => director.state == PlayState.Paused);
        onExecuteComplete?.Invoke();
    }
}