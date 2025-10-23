using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle/HintData")]
public class HintData : ScriptableObject
{
    [Tooltip("Номер подсказки")]
    public string hintNumber;

    [TextArea]
    [Tooltip("Текст подсказки")]
    public string hintText;
}