using UnityEngine;
using System.Collections;

public class Bullet_Default: MonoBehaviour
{
    protected Rigidbody2D rb;
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
    // store last velocity before physics step so we can restore it if a collision is ignored
    private Vector2 lastVelocity;

    private float pausedTime;

    private float OriLifeTime;

    

    public void InitBullet(float bulletSpeed, float bulletLifeTime, float bulletDamage, string bulletType, HSLColor color)
    {
        this.bulletSpeed = bulletSpeed;
        rb.linearVelocity = transform.right * bulletSpeed;
        this.bulletLifeTime = bulletLifeTime;
        this.bulletDamage = bulletDamage;
        this.bulletType = bulletType;
        this.bulletColor = color;
        currentState = BulletState.Flying;
    }

    protected virtual void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {
        UpdateBulletColor();

        Destroy(gameObject, bulletLifeTime);
    }

    protected virtual void FixedUpdate()
    {
        // keep track of the last velocity before any collision resolution
        if (rb != null)
        {
            lastVelocity = rb.linearVelocity;
        }
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

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
        {
            Shield shield = collision.collider.gameObject.GetComponent<Shield>();
            shield.takeDamage(bulletDamage);
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && bulletType == "player")
        {
            EnemyStats enemyStats = collision.gameObject.GetComponent<EnemyStats>();
            enemyStats.takeDamage(bulletDamage);
        }
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") && bulletType == "enemy")
        {
            PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();

            if (playerStats.isInvincible)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D playerCol = collision.collider as Collider2D;
                if (myCol != null && playerCol != null)
                {
                    Physics2D.IgnoreCollision(myCol, playerCol);
                }

                if (rb != null)
                {
                    rb.linearVelocity = lastVelocity;
                }

                // don't apply damage or destroy the bullet; let it pass through
                return;
            }

            playerStats.TakeDamage(bulletDamage);

            float influence = 0.01f;
            playerStats.ChangeWeaponType(bulletColor.H, influence);

        }
        Destroy(gameObject);
    }
//********************************Bullet Pause********************************
    public void PauseBullet(float pauseDuration, float pauseStrength)
    {
        if (currentState == BulletState.Flying)
        {
            StartCoroutine(PauseCoroutine(pauseDuration, pauseStrength));
        }
    }

    private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength)
    {
        savedVelocity = rb.linearVelocity;
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color colorBeforePause = spriteRenderer.color;
        rb.linearVelocity /= pauseStrength;
        currentState = BulletState.Paused;

        yield return new WaitForSeconds(pauseDuration);

        spriteRenderer.color = colorBeforePause;
        ResumeBullet();
    }

    public void ResumeBullet()
    {
        GetComponent<Collider2D>().enabled = true;
        if (currentState == BulletState.Paused)
        {
            rb.linearVelocity = savedVelocity;
            currentState = BulletState.Flying;
            bulletLifeTime = OriLifeTime;
        }
    }
}
