using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Node", fileName = "DialogueNode")]
public class DialogueNode : ScriptableObject
{
    public int nodeId;

    [Header("Content")]
    public string speakerName;
    [TextArea] public string dialogueLine;
    public Sprite speakerIcon;
   
    [Header("Voice")]
    public AudioClip voiceClip;

    [Header("Cutscene")]
    public bool pauseCutsceneAtEnd;
    public bool resumeCutsceneOnStart;
    public bool blockContinueOnStart;
    public bool blockUntilCutsceneEnd;

    [Header("Flow")]
    public DialogueNode nextNode;            
    public bool isLast = false;

    public virtual void Execute(DialogueManager manager, System.Action onExecuteComplete)
    {
        onExecuteComplete?.Invoke();
    }
}