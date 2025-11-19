using UnityEngine;

public class NPC_Canvas : MonoBehaviour
{
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }
    private void Update()
    {
        // transform.LookAt(player.transform.position);
        // transform.rotation *= Quaternion.Euler(0, 180, 0);
        transform.rotation = Camera.main.transform.rotation;
    }
}
