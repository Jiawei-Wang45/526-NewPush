using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public PlayerControllerTest pcTest;
    public EnemyStats enemyStats;

    public EnemyWeaponData weapon;
    public Rigidbody2D rb;
    public GameManager gameManager;
    public GameObject BoundHealthbar;
    public float RotationSpeed = 15.0f;
    public float enemySpeed;
    private Vector2 movement;
    private float timeToFire = 0;
    private bool currentlyFiring = false;

    public bool isAttacking;


    private void Start()
    {
        pcTest = FindFirstObjectByType<PlayerControllerTest>();
        enemyStats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
        RefreshStats();
        timeToFire = weapon.fireRate - 0.6f;
    }
    public void RefreshStats()
    {
        enemySpeed = enemyStats.enemyMovementSpeed;
    }
    private void Update()
    {
        if (gameManager.isPlayerAlive)
        {
            Vector2 direction = pcTest.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            if (isAttacking)
            {
                pcTest.TakeDamage(enemyStats.enemyDamage);
            }
        }
    }
    private void FixedUpdate()
    {
        if (gameManager.isPlayerAlive)
        {
            rb.linearVelocity = transform.right * enemySpeed;
        }
        if (!currentlyFiring)
        {
            timeToFire += Time.deltaTime;
        }
        if (timeToFire >= weapon.fireRate)
        {
            timeToFire = 0;
            StartCoroutine(BeginFiringSequence());
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = false;
        }
    }

    IEnumerator BeginFiringSequence()
    {
        for (int i = 0; i < weapon.bulletPattern.fireCount; i++)
        {
            FireOnce(i);
            if (i < weapon.bulletPattern.fireCount - 1)
            {
                yield return new WaitForSeconds(weapon.bulletPattern.timeBetweenFiring);
            }
        }

    }

    private void FireOnce(int volleyIndex)
    {
        float baseAngle = transform.eulerAngles.z + weapon.bulletPattern.rotateBetweenFiring * volleyIndex;
        if (weapon.bulletPattern.bulletCount == 1)
        {
            if (weapon.bulletPattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Even)
            {
                CreateBullet(baseAngle);
            }
            else
            {
                CreateBullet(baseAngle + UnityEngine.Random.Range(-weapon.bulletPattern.firingAngle / 2, weapon.bulletPattern.firingAngle / 2));
            }
        }
        else
        {
            List<float> angleChanges = new List<float>();
            float angleChange = -weapon.bulletPattern.firingAngle / 2;
            float changeStep = weapon.bulletPattern.firingAngle / (weapon.bulletPattern.bulletCount - 1);
            switch (weapon.bulletPattern.bulletDistribution)
            {
                case BulletPattern.bulletDistributionTypes.Even:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChanges.Add(angleChange);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.SemiRandom:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        float randomAdjustment = UnityEngine.Random.Range(-changeStep / 3, changeStep / 3);
                        angleChanges.Add(angleChange + randomAdjustment);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Random:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChange = UnityEngine.Random.Range(-weapon.bulletPattern.firingAngle / 2, weapon.bulletPattern.firingAngle / 2);
                        angleChanges.Add(angleChange);
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Radial:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChange = 0 + (weapon.bulletPattern.firingAngle/weapon.bulletPattern.bulletCount * i);
                        angleChanges.Add(angleChange);
                    }
                    break;
            }
            foreach (float change in angleChanges)
            {
                if (weapon.bulletPattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Radial)
                {
                    CreateBullet(change + (volleyIndex * weapon.bulletPattern.rotateBetweenFiring));
                }
                else
                {
                    CreateBullet(baseAngle + change + (volleyIndex * weapon.bulletPattern.rotateBetweenFiring));   
                }
            }   
        }
    }

    private void CreateBullet(float angle)
    {
        Vector2 spawnVector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        float offsetDistance = GetComponent<Collider2D>().bounds.extents.magnitude * 0.9f;
        Vector3 spawnPosition = transform.position + (Vector3)(spawnVector * offsetDistance);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject spawnedBullet = Instantiate(weapon.bulletType, spawnPosition, rotation);
        Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
        bulletAttributes.InitBullet(weapon.bulletSpeed, weapon.bulletLifeTime, weapon.bulletDamage, "enemy");
    }

    
}
