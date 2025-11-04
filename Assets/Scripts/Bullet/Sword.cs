using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sword : MonoBehaviour
{
    public PlayerControllerTest pc;
    public float swordDamage;
    public HSLColor swordColor;
    private bool isSwinging = false;
    private bool isReflecting = false;
    public GameObject reflectedBullet;

    public virtual void InitSword(float swordDamage, HSLColor color)
    {
        this.swordDamage = swordDamage;
        this.swordColor = color;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSwordColor();
    }

    private void UpdateSwordColor()
    {
        // 获取子弹的SpriteRenderer组件并应用HSL颜色
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = swordColor.ToRGB();
        }
    }
    public void ChangeSwordAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        Color color= spriteRenderer.color;
        color.a= alpha;
        spriteRenderer.color = color;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if(!isSwinging) return;
        IDamagable damagable = collision.collider.gameObject.GetComponent<IDamagable>();
        if (damagable != null)
        {
            damagable.TakeDamage(swordDamage, swordColor);
        }


        if(collision.collider.gameObject.layer == LayerMask.NameToLayer("EnemyBullet") && isReflecting)
        {
            float distance = Vector2.Distance(transform.position, collision.collider.gameObject.transform.position);
            if(distance > 2.0f && distance < 3.0f)
            {
                Debug.Log("distance: " + distance);
                float speed = collision.collider.gameObject.GetComponent<Bullet_Default>().bulletSpeed;
                float damage = collision.collider.gameObject.GetComponent<Bullet_Default>().bulletDamage;
                Vector2 position = collision.collider.gameObject.transform.position;
                Quaternion rotation = collision.collider.gameObject.transform.rotation;
                rotation *= Quaternion.Euler(0, 0, 180.0f);
                GameObject spawnedBullet = Instantiate(reflectedBullet, position, rotation);
                Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
                bulletAttributes.InitBullet(speed, damage, swordColor, 0);
            }
        }
    }

    public void Swing(float swingDuration)
    {
        StartCoroutine(SwingCoroutine(swingDuration));
    }

    private IEnumerator SwingCoroutine(float swingDuration)
    {
        if(isSwinging) yield break;
        isSwinging = true;
        isReflecting = true;
        float timeElapsed = 0f;
        Debug.Log("Swinging");
        while (timeElapsed < swingDuration/2.0f)
        {
            timeElapsed += Time.deltaTime;
            transform.RotateAround(pc.transform.position, Vector3.forward, -180.0f * Time.deltaTime/(swingDuration/2.0f));
            yield return null;
        }
        isReflecting = false;
        while (timeElapsed < swingDuration)
        {
            timeElapsed += Time.deltaTime;
            transform.RotateAround(pc.transform.position, Vector3.forward, +180.0f * Time.deltaTime/(swingDuration/2.0f));
            yield return null;
        }
        isSwinging = false;
    }

    public bool Swinging()
    {
        return isSwinging;
    }
}
