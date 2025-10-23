using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Dialogue/Activate Villain Node", fileName = "ActivateVillainNode")]
public class ActivateVillainNode : DialogueNode
{
    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            FinalBattleManager battleManager = gameManager.GetCurrentBattleManager();
            if (battleManager != null)
            {
                battleManager.ActivateVillain();
            }
        }
        
        onExecuteComplete?.Invoke();
    }
}