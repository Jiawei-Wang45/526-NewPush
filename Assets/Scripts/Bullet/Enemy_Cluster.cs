using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Enemy_Cluster : Bullet_Default
{    
    public GameObject explosionEffect;
    
    public Bullet_Default childToSpawn;
    public int childNums;
    
    public float clusterSpread = 140f;

    [Header("childBullet attributes")]

    public float childBulletSpeed = 6;

    public float childBulletDamage = 3;
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(bulletDamage);
        }   
        if (!(collision.collider.gameObject.layer == LayerMask.NameToLayer("Player")))
        {
            AudioManager.instance.PlaySound("explosion");
            Vector2 reverseDir = -transform.right;
            float baseAngle = (Mathf.Atan2(reverseDir.y, reverseDir.x) * Mathf.Rad2Deg) - clusterSpread/2;
            float angleStep = childNums > 1 ? clusterSpread / (childNums - 1) : 0;
            for (int i=0;i<childNums;i++)
            {
                float angle = baseAngle + (angleStep * i);
                Bullet_Default child=Instantiate(childToSpawn, transform.position, Quaternion.Euler(0, 0, angle));
                child.InitBullet(childBulletSpeed, childBulletDamage);
            }
        }
        Destroy(gameObject);       
    }
    protected override void OnDestroy() { }

}


/*

    float baseAngle = Mathf.Atan2(reverseDir.y, reverseDir.x) * Mathf.Rad2Deg;

    float spread = 140f;
    float startAngle = baseAngle - spread / 2f;
    float angleStep = spread / (childNums - 1);

    for (int i = 0; i < childNums; i++)
    {
        float angle = startAngle + angleStep * i;
        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Instantiate(childBulletPrefab, transform.position, rot);
    }
*/