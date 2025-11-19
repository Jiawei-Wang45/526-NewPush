using System;
using System.Collections;
using Unity.Collections;
using UnityEngine;

public class Bullet_Laser_Test : Bullet_Default
{
    [SerializeField] private float bulletLifeTime;
    //fadeIn can't be larger than the bulletLifeTime
    [SerializeField] private float fadeInTime;
    [NonSerialized] private float fadeOutTime;
    [NonSerialized] private float maxDistance;
    [NonSerialized] private float timeScale=1;
    [NonSerialized] CapsuleCollider2D cd;
    [NonSerialized] SpriteRenderer spriteRenderer;
    

    protected override void Awake()
    {
        base.Awake();
        cd = GetComponent<CapsuleCollider2D>();
        spriteRenderer= GetComponent<SpriteRenderer>();
        fadeOutTime = bulletLifeTime - fadeInTime;
    }
    protected override void Start()
    {
        // override to not change rb.linearvelocity
        float targetDistance = maxDistance = bulletLifeTime * bulletSpeed;
        RaycastHit2D hit = Physics2D.Linecast(transform.position, transform.position + transform.right * maxDistance, LayerMask.GetMask("Wall"));
        if (hit)
        {
            targetDistance = hit.distance;
        }
        StartCoroutine(IncreaseLaserLengthRoutine(targetDistance));
    }
    private IEnumerator IncreaseLaserLengthRoutine(float targetDistance)
    {
        float timePassed = 0;
        float distance = 0;
        while(distance < targetDistance)
        {
            timePassed+= Time.deltaTime*timeScale;
            distance = timePassed * bulletSpeed;
            spriteRenderer.size = new Vector2(distance, spriteRenderer.size.y);
            cd.size = new Vector2(distance, cd.size.y);
            cd.offset = new Vector2(distance / 2, cd.offset.y);
            SetLaserAlpha(timePassed);
            yield return null;
        }
        while (timePassed< bulletLifeTime)
        {
            timePassed += Time.deltaTime * timeScale;
            SetLaserAlpha(timePassed);
            yield return null;
        }
        Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        IDamagable damagable = collision.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(bulletDamage);
        }
        //if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        //{
        //    isIncreasing = false;
        //}
    }
    private void SetLaserAlpha(float timePassed)
    {
        Color color = spriteRenderer.color;
        if (timePassed < fadeInTime)
        {
            
            color.a = timePassed / bulletLifeTime;
            
        }
        else
        {
            color.a = 1 - (timePassed - fadeInTime) / fadeOutTime;
        }
        spriteRenderer.color = color;
    }
    public override void PauseStart(float pauseStrength)
    {
        timeScale /= pauseStrength;
    }
    public override void PauseEnd()
    {
        timeScale = 1;
    }
}
