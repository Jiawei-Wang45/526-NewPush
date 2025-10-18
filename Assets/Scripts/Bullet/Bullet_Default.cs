using UnityEngine;
using System.Collections;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    public float bulletSpeed;
    public float bulletLifeTime;
    public float bulletDamage;

    public string bulletType;

    public enum BulletState
{
    Flying,
    Paused
}   

    public HSLColor bulletColor = new HSLColor(); 
    public BulletState currentState;
    private Vector2 savedVelocity;

    private float pausedTime;

    private float OriLifeTime;

    

    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType, HSLColor color)
    {
        this.bulletSpeed = bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
        this.bulletColor = color;
        currentState = BulletState.Flying;
    }
    private void Start()
    {
        rb= GetComponent<Rigidbody2D>();
        rb.linearVelocity = transform.right * bulletSpeed;

        UpdateBulletColor();

        Destroy(gameObject, bulletLifeTime);

    }

    private void UpdateBulletColor()
    {
        // 获取子弹的SpriteRenderer组件并应用HSL颜色
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor.ToRGB();
        }
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

            float influence = 0.01f;
            playerStats.ChangeWeaponType(bulletColor.H, influence);

        }
        Destroy(gameObject);
    }
//********************************Bullet Pause********************************
    public void PauseBullet(float pauseDuration)
    {
        if (currentState == BulletState.Flying)
        {
            StartCoroutine(PauseCoroutine(pauseDuration));
        }
    }

    private IEnumerator PauseCoroutine(float pauseDuration)
    {
        savedVelocity = rb.linearVelocity;
        rb.linearVelocity /= 2f;
        currentState = BulletState.Paused;

        yield return new WaitForSeconds(pauseDuration);

        ResumeBullet();
    }

    public void ResumeBullet()
    {
        if (currentState == BulletState.Paused)
        {
            rb.linearVelocity = savedVelocity;
            currentState = BulletState.Flying;
            bulletLifeTime = OriLifeTime;
        }
    }
}
