using UnityEngine;

public class WalkpathManager : MonoBehaviour
{
    public static WalkpathManager Instance;
    public Transform[] WalkPoints;

    void Awake()
    {
        Instance = this;
    }
}