using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : EnemyController, IDamagable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [NonSerialized] private SpriteRenderer parentSprite;
    public EnemyWeaponData[] bossWeaponList;
    private int activeWeaponIndex = 0;
    private float spawnedtime = 0.0f;
    private bool isVulnerable = false;

    protected override void Start()
    {
        base.Start();
        weapon = bossWeaponList[activeWeaponIndex];
        parentSprite = GetComponent<SpriteRenderer>();
        isVulnerable = false;
        Color color = parentSprite.color;
        color.a = 0.5f;
        parentSprite.color = color;
    }
    protected override void FixedUpdate()
    {
        if(spawnedtime < 4.0f)
        {
            spawnedtime += Time.deltaTime;
            if(spawnedtime >= 4.0f)
            {
                isVulnerable = true;
                Color color = parentSprite.color;
                color.a = 1.0f;
                parentSprite.color = color;
            }
        }
        base.FixedUpdate();
    }

    new public void TakeDamage(float damage)
    {
        if (isVulnerable)
        {
            int life = enemyStats.life;
            enemyStats.TakeDamage(damage);
            if (enemyStats.life < life)
            {
                StopCoroutine(firingCoroutine);
                currentlyFiring = false;
                timeToFire = 0;
                spawnedtime = 0.0f; 
                isVulnerable = false;
                Color color = parentSprite.color;
                color.a = 0.5f;
                parentSprite.color = color;
                activeWeaponIndex++;
                weapon = bossWeaponList[activeWeaponIndex];
            }
        }
    }

    private IEnumerator haltCouroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

}
