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
    public string personality;
    public Gender gender;
    public AIVoice aiVoice;
}