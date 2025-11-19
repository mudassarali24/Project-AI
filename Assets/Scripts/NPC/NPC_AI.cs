using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum NPCState
{
    Idle,
    Talking,
    Walking
}

public class NPC_AI : MonoBehaviour
{
    [Header("Settings")]
    public NPC_Data npcData;
    public NPCState currentState;
    public DialogueSystem dialogueSystem;
    public float playerDistanceThreshold = 2f;
    private Animator animator;
    private NavMeshAgent agent;
    public float agentWaitAtDest = 4.0f;
    private float agentWait = 0.0f;
    private Vector3 currentWP;
    private bool playerEntered = false;
    private bool canMove = true;

    private GameObject player;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        ExecuteNPC();
    }

    private void Update()
    {
        DetectPlayerInRange();
        DetectInteractionInput();
        NPCMovement();
        LookToPlayerInIdle();
    }

    private void ExecuteNPC()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                animator.SetBool("Idle", true);
                break;
            case NPCState.Talking:
                animator.SetTrigger("Talking");
                animator.SetInteger("Talking_Ges", Random.Range(0, 2));
                break;
            case NPCState.Walking:
                animator.SetBool("Idle", false);
                animator.SetBool("Walking", true);
                break;
        }
    }

    private void LookToPlayerInIdle()
    {
        if (currentState != NPCState.Idle) return;
        if (!playerEntered) return;
        Vector3 targetPos = player.transform.position;
        targetPos.y = transform.position.y;
        transform.LookAt(targetPos);
    }

    private void DetectPlayerInRange()
    {
        if (!playerEntered) return;
        if (Vector3.Distance(transform.position, player.transform.position) > playerDistanceThreshold)
        {
            playerEntered = false;
            if (dialogueSystem.interactPanelActive) dialogueSystem.ToggleInteractPanel(false);
        }
    }

    private void DetectInteractionInput()
    {
        if (playerEntered && Input.GetKeyDown(KeyCode.E))
        {
            // Interaction should happen
            ChangeState(NPCState.Idle);
            if (!agent.isStopped) canMove = false;
            if (dialogueSystem.interactPanelActive) dialogueSystem.ToggleInteractPanel(false);
            dialogueSystem.ToggleDialoguePanel(true);
            StartCoroutine(TestDialogueSystem());
        }
    }

    #region NPC_MOVEMENT

    private void NPCMovement()
    {
        if (npcData.type != NPCType.Walkable) return;
        if (!canMove) return;
        if (currentWP == Vector3.zero) currentWP = WalkpathManager.Instance.WalkPoints[Random.Range(0, WalkpathManager.Instance.WalkPoints.Length)].position;
        agent.SetDestination(currentWP);
        if (agent.remainingDistance <= 0.4f)
        {
            currentState = NPCState.Idle;
            agentWait += Time.deltaTime;
            if (agentWait >= agentWaitAtDest)
            {
                // Move to next destination
                GetNextDestination();
                agentWait = 0.0f;
                // agent.SetDestination(currentWP);
            }
        }
        else
        {
            currentState = NPCState.Walking;
            agentWait = 0.0f;
        }
        ExecuteNPC();
    }

    private Vector3 GetNextDestination()
    {
        Vector3 previousWP = Vector3.zero;
        do
        {
            previousWP = currentWP;
            currentWP = WalkpathManager.Instance.WalkPoints[Random.Range(0, WalkpathManager.Instance.WalkPoints.Length)].position;
        }
        while (previousWP == currentWP);
        return currentWP;
    }

    #endregion
    private IEnumerator TestDialogueSystem()
    {
        dialogueSystem.AnimateDots();
        yield return new WaitForSeconds(5.0f);
        dialogueSystem.AnimateText("Hello, this is a dummy text for the dialogue system!");
    }

    public void AttachToPlayer()
    {
        playerEntered = true;
        if (dialogueSystem.interactPanelActive || dialogueSystem.dialoguePanelActive) return;
        dialogueSystem.ToggleInteractPanel(true);
    }

    public void ChangeState(NPCState state)
    {
        currentState = state;
        ExecuteNPC();
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDistanceThreshold);
    }

#endif
}