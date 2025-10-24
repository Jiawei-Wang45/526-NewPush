using UnityEngine;
using System.Collections;
using UnityEditor.UIElements;

public class Bullet_Default: MonoBehaviour
{
    private Rigidbody2D rb;
    private PauseAbility playerPauseAbility;
    public float bulletSpeed;
    //public float bulletLifeTime;
    public float bulletDamage;

    //public string bulletType;

    public GameObject ClusterBullet;

//    public enum BulletState
//{
//    Flying,
//    Paused
//}   

    public HSLColor bulletColor = new HSLColor(); 


    //SpeedFactor is used during pause time for the enemy to slow the bullets down, In other case it's 1 by default
    public void InitBullet(float bulletSpeed, float bulletDamage, HSLColor color, float slowFactor=1.0f)
    {
        this.bulletSpeed = bulletSpeed;
        rb.linearVelocity = transform.right * bulletSpeed/ slowFactor;
        this.bulletDamage = bulletDamage;
        this.bulletColor = color;
        //this.bulletLifeTime = bulletLifeTime;
        //this.bulletType = bulletType;
        //currentState = BulletState.Flying;
    }

    private void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
        PauseAbility.instance.OnPauseStart += PauseStart;
        PauseAbility.instance.OnPauseEnd += PauseEnd;
    }

    private void Start()
    {
        UpdateBulletColor();
        
    }

    private void Instance_OnPauseEnd()
    {
        throw new System.NotImplementedException();
    }

    //private void FixedUpdate()
    //{
    //    // keep track of the last velocity before any collision resolution
    //    if (rb != null)
    //    {
    //        lastVelocity = rb.linearVelocity;
    //    }
    //}

    private void UpdateBulletColor()
    {
        // 获取子弹的SpriteRenderer组件并应用HSL颜色
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = bulletColor.ToRGB();
        }
    }
    public void ChangeBulletAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color color= spriteRenderer.color;
        color.a= alpha;
        spriteRenderer.color = color;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("Object is " + collision.gameObject.name+ "Collider is " + collision.collider.name+ "Bullet layer is " + gameObject.layer+"collision layer is "+ collision.gameObject.layer);
        //collision represents the parent gameobject, while the collider stands for the actual collision object the bullet hits, take care!
        IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(bulletDamage, bulletColor);
        }
        Destroy(gameObject);
        //if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Shield"))
        //{
        //    Shield shield = collision.collider.gameObject.GetComponent<Shield>();
        //    shield.TakeDamage(bulletDamage);
        //}
        //if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && bulletType == "player")
        //{
        //    EnemyStats enemyStats = collision.gameObject.GetComponent<EnemyStats>();
        //    enemyStats.takeDamage(bulletDamage);
        //}
        //if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Player") && bulletType == "enemy")
        //{
        //    PlayerStats playerStats = collision.gameObject.GetComponent<PlayerStats>();

        //    //if (playerStats.isInvincible)
        //    //{
        //    //    Collider2D myCol = GetComponent<Collider2D>();
        //    //    Collider2D playerCol = collision.collider as Collider2D;
        //    //    if (myCol != null && playerCol != null)
        //    //    {
        //    //        Physics2D.IgnoreCollision(myCol, playerCol);
        //    //    }

        //    //    if (rb != null)
        //    //    {
        //    //        rb.linearVelocity = lastVelocity;
        //    //    }

        //    //    // don't apply damage or destroy the bullet; let it pass through
        //    //    return;
        //    //}

        //    playerStats.TakeDamage(bulletDamage);

        //    float influence = 0.01f;
        //    playerStats.ChangeWeaponType(bulletColor.H, influence);

        //}

    }
    //********************************Bullet Pause********************************
    //public void Pause(float pauseDuration, float pauseStrength)
    //{
    //    //if (currentState == BulletState.Flying)
    //    //{
    //    //    StartCoroutine(PauseCoroutine(pauseDuration, pauseStrength));
    //    //}
    //    StartCoroutine(PauseCoroutine(pauseDuration, pauseStrength));
    //}

    //private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength)
    //{
    //    //savedVelocity = rb.linearVelocity;
    //    rb.linearVelocity /= pauseStrength;
    //    //currentState = BulletState.Paused;

    //    yield return new WaitForSeconds(pauseDuration);

    //    ResumeBullet();
    //}

    //public void ResumeBullet()
    //{
    //    //if (currentState == BulletState.Paused)
    //    //{
    //    //    rb.linearVelocity = savedVelocity;
    //    //    currentState = BulletState.Flying;
    //    //}
    //    rb.linearVelocity = transform.right * bulletSpeed;
    //}
    public void PauseStart(float pauseStrength)
    {
        rb.linearVelocity /= pauseStrength;
    }
    public void PauseEnd()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
    }
    private void OnDestroy()
    {
        PauseAbility.instance.OnPauseStart -= PauseStart;
        PauseAbility.instance.OnPauseEnd -= PauseEnd;
    }
}
