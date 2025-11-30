using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet_Cluster : Bullet_Default
{
    [SerializeField] private bool isParentBullet = false;
    [Header("parentBullet attributes ")]
    public float parentBulletDamage;
    public float parentBulletSpeed;
    public float blastRadius;
    public GameObject explosionEffect;
    public float initialSpeedForChild;
    public Bullet_Cluster childToSpawn;
    public int childNums;
    public LayerMask enemyLayer;
    [Header("childBullet attributes")]
    public float dampingFactor;
    public float minBulletSpeed;
    private float accumulateTime = 0;
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    protected override void FixedUpdate()
    {
        if (!isParentBullet)
        {
            rb.linearVelocity = transform.right * (minBulletSpeed + (bulletSpeed - minBulletSpeed) * Mathf.Exp(-dampingFactor * accumulateTime));
            accumulateTime += Time.fixedDeltaTime;
        }
        
    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        if (isParentBullet)
        {
            Collider2D[] enemiesInRadius = Physics2D.OverlapCircleAll(transform.position, blastRadius, enemyLayer);
            foreach(Collider2D enemy in enemiesInRadius)
            {
                IDamagable damagable = enemy.gameObject.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    damagable.TakeDamage(parentBulletDamage);
                }
            }

            AudioManager.instance.PlaySound("explosion");
            Instantiate(explosionEffect, transform.position, new Quaternion());
            //spawn children
            for (int i=0;i<childNums;i++)
            {
                float angle = Random.Range(0, 2 * Mathf.PI)*Mathf.Rad2Deg;
                Bullet_Cluster child=Instantiate(childToSpawn, transform.position, Quaternion.Euler(0, 0, angle));
                child.InitBullet(initialSpeedForChild, bulletDamage / 10);

            }
            Destroy(gameObject);
        }
        else
        {
            base.OnCollisionEnter2D(collision);
        }         
    }
    protected override void OnDestroy() { }

}
