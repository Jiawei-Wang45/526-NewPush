using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ParryHitbox : MonoBehaviour
{
    [SerializeField] private float reflectWindowTime = 0.3f;

    private Collider2D hitbox;
    private float startTime;
    private bool isActive;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
    }

    public void EnableParry()
    {
        Debug.Log("EnableParry called!");
        startTime = Time.time;
        isActive = true;
        hitbox.enabled = true;
    }

    public void DisableParry()
    {
        isActive = false;
        hitbox.enabled = false;
    }

    public void SyncToSprite(Sprite sprite)
    {
        if (!(hitbox is BoxCollider2D box) || sprite == null) return;

        var bounds = sprite.bounds;                 
        box.size = bounds.size;
        box.offset = bounds.center;                 
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log($"OnTriggerEnter2D called! isActive: {isActive}, Layer: {collider.gameObject.layer}, Name: {collider.gameObject.name}");
        if (!isActive) return;
        if (collider.gameObject.layer != LayerMask.NameToLayer("EnemyBullet")) return;

        var enemyBullet = collider.GetComponent<Bullet_Default>();
        if (enemyBullet == null) return;

        if (Time.time - startTime <= reflectWindowTime)
        {
            enemyBullet.ReflectBullet();
        }
        else
        {
            Destroy(collider.gameObject);
        }
    }
}