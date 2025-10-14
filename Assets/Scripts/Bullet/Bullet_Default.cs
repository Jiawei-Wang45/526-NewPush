using UnityEngine;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletDamage;
    public string bulletType;
    public SpriteRenderer spriteRenderer;
    public float enemyHue = 0f; // 敌人色相，用于传递色相信息

    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
    }
    
    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType, Color bulletColor)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
        
        // 设置子弹颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor;
        }
    }
    
    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType, Color bulletColor, float enemyHue)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
        this.enemyHue = enemyHue; // 存储敌人色相
        
        // 设置子弹颜色
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor;
        }
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
            
            // 应用色相偏移
            PlayerHSLSystem playerHSL = collision.gameObject.GetComponent<PlayerHSLSystem>();
            if (playerHSL != null)
            {
                float shiftAmount = 0.1f; // 每次击中偏移10%
                playerHSL.ApplyHueShift(enemyHue, shiftAmount);
            }
        }
        Destroy(gameObject);
    }
}
