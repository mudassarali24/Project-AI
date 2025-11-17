using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    [Header("Settings")]
    

    [Space]
    public float npcDetectRadius;
    public LayerMask npcLayerMask;
    private bool npcInRadius = false;

    void Update()
    {
        CheckForNPC();
    }

    private void CheckForNPC()
    {
        npcInRadius = Physics.CheckSphere(transform.position, npcDetectRadius, npcLayerMask);
        if (npcInRadius)
        {
            NPC_AI[] allNPC = Physics.SphereCastAll(transform.position, npcDetectRadius, npcDetectRadius);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, npcDetectRadius);
    }

#endif
}