using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : EnemyController, IDamagable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public EnemyWeaponData[] bossWeaponList;
    private int activeWeaponIndex = 0;

    protected override void Start()
    {
        base.Start();
        weapon = bossWeaponList[activeWeaponIndex];
    }
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    new public void TakeDamage(float damage)
    {
        int life = enemyStats.life;
        enemyStats.TakeDamage(damage);
        if (enemyStats.life < life)
        {
            activeWeaponIndex++;
            weapon = bossWeaponList[activeWeaponIndex];
        }
    }

    private IEnumerator haltCouroutine(float seconds)
    {
        yield return new WaitForSeconds(seconds);
    }

}
