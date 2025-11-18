using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet_Laser : Bullet_Default
{
    private float livedTime = 0.0f;
    private float spinUpTime = 0.0f;
    private float currentAlpha = 0.0f;
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        // if (damagable != null && livedTime >= spinUpTime)
        // {
        //     damagable.TakeDamage(bulletDamage);
        // }

    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        Debug.Log("Triggered");
        IDamagable damagable = collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null && livedTime >= spinUpTime)
        {
            damagable.TakeDamage(bulletDamage);
        }

    }

    public void InitBulletwithSpinup(float bulletSpeed, float bulletDamage, int bounce = 0, float spinup = 0.0f)
    {
        InitBullet(bulletSpeed, bulletDamage, bounce);
        spinUpTime = spinup;
        ChangeBulletAlpha(0.5f);
        currentAlpha = 0.5f;
    }

    protected override void Awake()
    {
        base.Awake();
        livedTime = 0.0f;
    }

    private void FixedUpdate()
    {
        livedTime += Time.fixedDeltaTime;
        if (livedTime > spinUpTime)
        {
            StartCoroutine(LaserAppear(0.2f));
            currentAlpha = 1.0f;
        }
        if (livedTime - spinUpTime > 2.0f)
        {
            StartCoroutine(LaserDisappear(0.4f));
        }
    }

    private IEnumerator LaserDisappear(float fadeDuration)
    {
        GetComponent<Collider2D>().enabled = false;
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, 0f, timeElapsed / fadeDuration);
            ChangeBulletAlpha(newAlpha);
            yield return null;
        }
        // just let it die
        Destroy(gameObject);
    }

    private IEnumerator LaserAppear(float fadeDuration)
    {
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(currentAlpha, 1.0f, timeElapsed / fadeDuration);
            ChangeBulletAlpha(newAlpha);
            yield return null;
        }
    }

}
