using UnityEngine;

// Simple 2D win trigger. Attach to a GameObject with a Collider2D set as IsTrigger.
// When the player enters the trigger, this will call GameManager.PlayerReachedWinTrigger().
[RequireComponent(typeof(Collider2D))]
public class WinTrigger : MonoBehaviour
{
    private void Reset()
    {
        // Ensure the collider is configured as a trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.GetComponentInParent<PlayerControllerTest>();
        if (player != null)
        {
            Debug.Log($"[WinTrigger] Player reached win trigger on '{gameObject.name}'");
            GameManager.instance?.PlayerReachedWinTrigger();
            // Optionally disable the trigger so it doesn't fire repeatedly
            enabled = false;
        }
    }
}
