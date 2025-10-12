using UnityEngine;

// Simple component to store extra data on treasure pickups (enemy hue, etc.)
public class TreasureProps : MonoBehaviour
{
    [Tooltip("Hue in degrees 0-360 copied from the enemy when spawned")]
    public float hue = 0f;
}
