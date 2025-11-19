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
            RaycastHit[] allNPC = Physics.SphereCastAll(transform.position, npcDetectRadius, transform.forward, npcDetectRadius, npcLayerMask);

            NPC_AI closestNpc = null;
            float closestDistance = 0.0f;

            if (allNPC.Length > 0)
            {
                // Iterate over all NPCs in radius and select the closest one
                for (int i = 0; i < allNPC.Length; i++)
                {
                    if (closestNpc == null)
                    {
                        closestNpc = allNPC[0].transform.GetComponent<NPC_AI>();
                        closestDistance = Vector3.Distance(transform.position, closestNpc.transform.position);
                        continue;
                    }

                    if (Vector3.Distance(transform.position, allNPC[i].transform.position) < closestDistance)
                    {
                        closestNpc = allNPC[i].transform.GetComponent<NPC_AI>();
                        closestDistance = Vector3.Distance(transform.position, closestNpc.transform.position);
                    }
                }
            }

            if (closestNpc != null)
            {
                // we have the closest npc or at least one npc
                closestNpc.AttachToPlayer();
            }
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