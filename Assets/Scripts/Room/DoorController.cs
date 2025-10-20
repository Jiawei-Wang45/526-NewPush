using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Collider2D doorCollider;
    public SpriteRenderer doorRenderer;

    private void Start()
    {
        if (!doorCollider)
            doorCollider = GetComponent<Collider2D>();
        if (!doorRenderer)
            doorRenderer = GetComponent<SpriteRenderer>();

        if (doorCollider) doorCollider.enabled = false;
        if (doorRenderer) doorRenderer.enabled = false;
    }

    public void Lock()
    {
        if (doorCollider) doorCollider.enabled = true;
        if (doorRenderer) doorRenderer.enabled = true;
    }

    public void Unlock()
    {
        if (doorCollider) doorCollider.enabled = false;
        if (doorRenderer) doorRenderer.enabled = false;
    }
}
