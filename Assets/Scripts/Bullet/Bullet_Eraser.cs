using UnityEngine;

public class Bullet_Eraser: Bullet_Default
{
    protected override void Awake()
    {
        rb= GetComponent<Rigidbody2D>();
    }
    protected override void Start()
    {
        rb.linearVelocity = transform.right * 50;
        currentState = BulletState.Flying;

        Destroy(gameObject, 0.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBullet"))
        {
            Bullet_Default enemyBullet = collision.gameObject.GetComponent<Bullet_Default>();
            if (enemyBullet.currentState == BulletState.Paused)
            {
                SpriteRenderer spriteRenderer = collision.gameObject.GetComponent<SpriteRenderer>();
                Color bulletColor = spriteRenderer.color;
                spriteRenderer.color = new Color(bulletColor.r, bulletColor.g, bulletColor.b, 0.2f);
                collision.gameObject.GetComponent<Collider2D>().enabled = false;
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            collision.gameObject.GetComponent<EnemyController>().Stun();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (transform.localScale.x < 12f)
        {
            transform.localScale *= 1.05f;
        }
    }
}