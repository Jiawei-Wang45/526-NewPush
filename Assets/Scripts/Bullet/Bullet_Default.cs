using UnityEngine;
using System.Collections;


public class Bullet_Default: MonoBehaviour
{
    protected Rigidbody2D rb;
    //private PauseAbility playerPauseAbility;
    public float bulletSpeed;
    public float bulletDamage;
    public int bounceCount = 0;
    public float bounceMultiplier = 2.0f;


    //SpeedFactor is used during pause time for the enemy to slow the bullets down, In other case it's 1 by default
    public virtual void InitBullet(float bulletSpeed, float bulletDamage, int bounce = 0)
    {
        this.bulletSpeed = bulletSpeed;      
        this.bulletDamage = bulletDamage;
        this.bounceCount = bounceCount > 0 ? bounceCount : bounce;
    }

    protected virtual void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
        PauseManager.instance.OnPauseStart += PauseStart;
        PauseManager.instance.OnPauseEnd += PauseEnd;
    }
    protected virtual void Start()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
    }
    //private void Instance_OnPauseEnd()
    //{
    //    throw new System.NotImplementedException();
    //}

    public void ChangeBulletAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color color= spriteRenderer.color;
        color.a= alpha;
        spriteRenderer.color = color;
    }
    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(bulletDamage);
        }

        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.collider.gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            if(bounceCount > 0)
            {
                bounceCount--;
                bulletDamage *= bounceMultiplier;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Bounce(Vector2 normalVector)
    {
        rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normalVector);
    }
    
    public virtual void PauseStart(float pauseStrength)
    {
        rb.linearVelocity /= pauseStrength;
    }
    public virtual void PauseEnd()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
    }
    protected virtual void OnDestroy()
    {
        PauseManager.instance.OnPauseStart -= PauseStart;
        PauseManager.instance.OnPauseEnd -= PauseEnd;
    }
    public void ReflectBullet()
    {
        rb.linearVelocity = -rb.linearVelocity;
        gameObject.layer = LayerMask.NameToLayer("PlayerBullet");
    }
}
