using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet_Laser : Bullet_Default
{
    private float livedTime = 0.0f;
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(bulletDamage, bulletColor);
        }

    }

    protected override void Awake()
    {
        base.Awake();
        livedTime = 0.0f;
    }

    private void Update()
    {
        livedTime += Time.fixedDeltaTime;
        if (livedTime > 6.0f)
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
            float newAlpha = Mathf.Lerp(0.5f, 0f, timeElapsed / fadeDuration);
            ChangeBulletAlpha(newAlpha);
            yield return null;
        }
        // just let it die
        Destroy(gameObject);
    }

}
