using UnityEngine;

public class BafflePlate : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private string bulletTag;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        bulletTag = "TutorialBullet";
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(bulletTag))
        {
            Destroy(collision.gameObject);
        }
    }
    public void SetVisibility(bool isVisible)
    {
        if (isVisible)
        {
            spriteRenderer.enabled = true;
        }
        else
        {
            spriteRenderer.enabled = false;
        }
    }
}
