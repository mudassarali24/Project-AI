using UnityEngine;

public enum NPCState
{
    Idle,
    Listening,
    Thinking,
    Talking,
    Walking
}

public class NPC_AI : MonoBehaviour
{
    [Header("Settings")]
    public NPC_Data npcData;
    public NPCState currentState;
    public float playerDistanceThreshold = 2f;
    private Animator animator;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        animator = GetComponent<Animator>();
        ExecuteNPC();
    }

    // private void Update()
    // {
    //     ExecuteNPC();
    // }

    private void ExecuteNPC()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                animator.SetTrigger("Idle");
                break;
            case NPCState.Listening:
                animator.SetTrigger("Idle");
                break;
            case NPCState.Talking:
                animator.SetTrigger("Talking");
                animator.SetInteger("Talking_Ges", Random.Range(0, 2));
                break;
            case NPCState.Walking:
                animator.SetTrigger("Walking");
                break;
        }
    }

    public void ChangeState(NPCState state)
    {
        currentState = state;
        ExecuteNPC();
    }
}