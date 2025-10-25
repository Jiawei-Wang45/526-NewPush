using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bullet_Cluster : Bullet_Default
{
    [SerializeField] private bool isParentBullet = false;
    [Header("parentBullet attributes ")]
    public float parentBulletDamage;
    public float parentBulletSpeed;
    public GameObject explosionEffect;
    public float initialSpeedForChild;
    public Bullet_Cluster childToSpawn;
    public int childNums;

    [Header("childBullet attributes")]
    public float dampingFactor;
    public float minBulletSpeed;
    private float accumulateTime = 0;
    protected override void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void FixedUpdate()
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
            AudioManager.instance.PlaySound("explosion");
            Instantiate(explosionEffect, transform.position, new Quaternion());
            //spawn children
            for (int i=0;i<childNums;i++)
            {
                float angle = Random.Range(0, 2 * Mathf.PI)*Mathf.Rad2Deg;
                Bullet_Cluster child=Instantiate(childToSpawn, transform.position, Quaternion.Euler(0, 0, angle));
                child.InitBullet(initialSpeedForChild, bulletDamage / 10, bulletColor);

            }   
        }

        base.OnCollisionEnter2D(collision);
    }
    protected override void OnDestroy() { }

}
