using UnityEngine;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletDamage;

    public void InitBullet(float bulletSpeed, float bulletLifeTime,float bulletDamage)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
    }
    private void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * bulletSpeed;
        Destroy(gameObject, bulletLifeTime);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<EnemyStats>(out EnemyStats enemyScript))
        {
            enemyScript.takeDamage(bulletDamage);
        }
        Destroy(gameObject);
    }
}
