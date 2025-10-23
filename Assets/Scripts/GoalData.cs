using UnityEngine;

[CreateAssetMenu(menuName = "Puzzle/GoalData")]
public class GoalData : ScriptableObject
{
    [TextArea]
    public string goalText;
}