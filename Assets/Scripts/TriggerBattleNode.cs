using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu(menuName = "Dialogue/Trigger Battle Node", fileName = "TriggerBattleNode")]
public class TriggerBattleNode : DialogueNode
{
    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        onExecuteComplete?.Invoke();
    }

    public void StartBattle(DialogueManager manager, Action onBattleComplete)
    {
        manager.StartCoroutine(BattleCoroutine(manager, onBattleComplete));
    }

    private IEnumerator BattleCoroutine(DialogueManager manager, Action onBattleComplete)
    {
        var dialogueView = FindObjectOfType<DialogueView>();
        if (dialogueView != null)
        {
            dialogueView.Hide();
        }

        yield return new WaitForSeconds(0.5f);

        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            onBattleComplete?.Invoke();
            yield break;
        }

        FinalBattleManager battleManager = gameManager.GetCurrentBattleManager();
        if (battleManager == null)
        {
            onBattleComplete?.Invoke();
            yield break;
        }

        if (battleManager.GetBattleInput() != null)
        {
            battleManager.GetBattleInput().enabled = true;
        }

        bool isBattleFinished = false;
        Action onFinish = () => { isBattleFinished = true; };

        battleManager.OnBattleFinished += onFinish;
        battleManager.TriggerItemPhase();

        yield return new WaitUntil(() => isBattleFinished);

        battleManager.OnBattleFinished -= onFinish;
        
        onBattleComplete?.Invoke();
    }
}