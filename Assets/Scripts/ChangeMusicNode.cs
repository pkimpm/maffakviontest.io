using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Dialogue/Change Music Node", fileName = "ChangeMusicNode")]
public class ChangeMusicNode : DialogueNode
{
    [Header("Музыка")]
    [Tooltip("Имя музыки из SoundLibrary")]
    public string musicName;
    
    [Tooltip("Длительность кроссфейда")]
    public float fadeDuration = 2f;

    public override void Execute(DialogueManager manager, Action onExecuteComplete)
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicName))
        {
            AudioManager.Instance.CrossfadeMusic(musicName, fadeDuration);
        }
        
        onExecuteComplete?.Invoke();
    }
}