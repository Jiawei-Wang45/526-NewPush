using UnityEngine;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletDamage;
    public string bulletType;
    public string bulletColor;
    public ColorTable bulletColorTable;
    public SpriteRenderer spriteRenderer;

    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType,string bulletColor)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
        this.bulletColor=bulletColor;
        spriteRenderer.color = bulletColorTable.GetColorByName(bulletColor);
 
    }
    private void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * bulletSpeed;
        Destroy(gameObject, bulletLifeTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && bulletType == "player")
        {
            EnemyStats enemyStats = collision.gameObject.GetComponent<EnemyStats>();
            float multiplier = bulletColorTable.GetMultiplier(enemyStats.color, bulletColor);
            enemyStats.takeDamage(multiplier*bulletDamage);
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && bulletType == "enemy")
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.TakeDamage(bulletDamage);
        }
        Destroy(gameObject);
    }
}
