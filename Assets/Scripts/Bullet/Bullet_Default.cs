using UnityEngine;
using System.Collections;
//using UnityEditor.UIElements;

public class Bullet_Default: MonoBehaviour
{
    protected Rigidbody2D rb;
    private PauseAbility playerPauseAbility;
    public float bulletSpeed;
    public float bulletDamage;
    public int bounceCount = 0;

    public HSLColor bulletColor = new HSLColor(); 


    //SpeedFactor is used during pause time for the enemy to slow the bullets down, In other case it's 1 by default
    public virtual void InitBullet(float bulletSpeed, float bulletDamage, HSLColor color, int bounce = 0)
    {
        this.bulletSpeed = bulletSpeed;
        rb.linearVelocity = transform.right * bulletSpeed;
        this.bulletDamage = bulletDamage;
        this.bulletColor = color;
        this.bounceCount = bounce;
    }

    protected virtual void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
        PauseManager.instance.OnPauseStart += PauseStart;
        PauseManager.instance.OnPauseEnd += PauseEnd;
    }

    private void Start()
    {
        UpdateBulletColor();
        
    }

    private void Instance_OnPauseEnd()
    {
        throw new System.NotImplementedException();
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
            damagable.TakeDamage(bulletDamage, bulletColor);
        }

        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if(bounceCount > 0)
            {
                bounceCount--;
                bulletDamage *= 2.0f;
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
    
    public void PauseStart(float pauseStrength)
    {
        rb.linearVelocity /= pauseStrength;
    }
    public void PauseEnd()
    {
        rb.linearVelocity = transform.right * bulletSpeed;
    }
    protected virtual void OnDestroy()
    {
        PauseManager.instance.OnPauseStart -= PauseStart;
        PauseManager.instance.OnPauseEnd -= PauseEnd;
    }
}
