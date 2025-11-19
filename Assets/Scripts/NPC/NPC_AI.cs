using System.Collections;
using Unity.VisualScripting;
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
    private AudioSource audioSource;
    public float agentWaitAtDest = 4.0f;
    private float agentWait = 0.0f;
    public float talkAnimateThreshold = 2.0f;
    private float talkAnimationTime = 0.0f;
    private Vector3 currentWP;
    private bool playerEntered = false;
    private bool canMove = true;
    private string aiReply;

    private GameObject player;

    void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player");
        ExecuteNPC();
    }

    private void Update()
    {
        DetectPlayerInRange();
        DetectInteractionInput();
        NPCMovement();
        LookToPlayerInIdle(); 
        AnimatePlayerInTalking();
    }

    private void ExecuteNPC()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                animator.SetBool("Idle", true);
                animator.SetBool("Walking", false);
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

    private void AnimatePlayerInTalking()
    {
        if (currentState != NPCState.Talking) return;
        talkAnimationTime += Time.deltaTime;
        if (talkAnimationTime >= talkAnimateThreshold)
        {
            animator.SetTrigger("Talking");
            animator.SetInteger("Talking_Ges", Random.Range(0, 2));
            talkAnimationTime = 0.0f;
        }
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
            // Stop if moving
            if (!agent.isStopped)
            {
                canMove = false;
                agent.isStopped = true;
                ChangeState(NPCState.Idle);
            }
            if (dialogueSystem.interactPanelActive) dialogueSystem.ToggleInteractPanel(false);
            dialogueSystem.ToggleDialoguePanel(true);
            // StartCoroutine(TestDialogueSystem());

            dialogueSystem.AnimateDots();
            OpenAIManager.npcPersonality = npcData.personality;
            OpenAIManager.npcVoice = npcData.aiVoice.ToString();
            OpenAIManager.OnAIReplyReceived += DisplayAIReply;
            OpenAIManager.OnSpeechReady += PlayNPCReply;
            AudioRecorder.Instance.StartRecording();
        }
    }

    private void DisplayAIReply(string _aiReply)
    {
        aiReply = _aiReply;
        // dialogueSystem.AnimateText(aiReply);
    }

    private void PlayNPCReply(AudioClip clip)
    {
        if (clip == null) return;
        dialogueSystem.AnimateText(aiReply);
        audioSource.clip = clip;
        currentState = NPCState.Talking;
        ExecuteNPC();
        audioSource.Play();
        StartCoroutine(WaitForClipEnd());
    }
    private IEnumerator WaitForClipEnd()
    {
        while (audioSource.isPlaying)
            yield return null;

        // Reset OpenAI

        OpenAIManager.npcPersonality = "";
        OpenAIManager.npcVoice = "";
        OpenAIManager.OnAIReplyReceived -= DisplayAIReply;
        OpenAIManager.OnSpeechReady -= PlayNPCReply;
        currentState = NPCState.Idle;
        ExecuteNPC();
        yield return new WaitForSeconds(4.0f);
        canMove = true;
        dialogueSystem.ToggleDialoguePanel(false);
        currentState = NPCState.Idle;
        ExecuteNPC();
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
        dialogueSystem.AnimateText("Hello, this is a dummy text for the dialogue system! And this is going to be a bit large just to make sure that the text fits in the dialogue box! If it does, then hurray! You are successfull!");
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