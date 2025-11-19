using UnityEngine;
public enum NPCType
{
    Idle,
    Walkable
}

public enum Gender
{
    Male,
    Female
}

public enum AIVoice
{
    alloy, ash, ballad, coral, echo, fable, onyx, nova, sage, verse
}

[CreateAssetMenu(fileName = "NPC_Data", menuName = "ScriptableObjects/NPC_Data")]
public class NPC_Data : ScriptableObject
{
    public NPCType type;
    [TextArea()]
    public string personality = "You are an NPC in a game. Reply as a game character without breaking immersion in only one or two lines.";
    public Gender gender;
    public AIVoice aiVoice;
}