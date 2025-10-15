using UnityEngine;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletDamage;

    public string bulletType;

    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
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
            enemyStats.takeDamage(bulletDamage);
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && bulletType == "enemy")
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();
            playerStats.TakeDamage(bulletDamage);
        }
        Destroy(gameObject);
    }
}
