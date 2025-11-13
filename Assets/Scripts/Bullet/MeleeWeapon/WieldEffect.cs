using System;
using UnityEngine;
using static WieldEffect;

public class WieldEffect: MonoBehaviour
{
    public enum EffectType
    {
        Slash,
        Reflect
    }
    [NonSerialized] private int ENEMY_LAYER;
    [NonSerialized] private int ENEMYBULLET_LAYER;
    [NonSerialized] private float damage;
    [NonSerialized] private EffectType effectType;
    private void Awake()
    {
        ENEMY_LAYER = LayerMask.NameToLayer("Enemy");
        ENEMYBULLET_LAYER = LayerMask.NameToLayer("EnemyBullet");
    }
    public void Init(EffectType ineffectType, float inDamage)
    {
        effectType=ineffectType;
        damage = inDamage;
    }
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (effectType == EffectType.Slash)
        {
            if (collider.gameObject.layer == ENEMY_LAYER)
            {
                collider.gameObject.GetComponent<IDamagable>().TakeDamage(damage);
                return;
            }
            if (collider.gameObject.layer == ENEMYBULLET_LAYER)
            {
                Destroy(collider.gameObject);
            }
        }
        else
        {
            if (collider.gameObject.layer == ENEMYBULLET_LAYER)
            {
                Bullet_Default enemyBullet = collider.gameObject.GetComponent<Bullet_Default>();
                enemyBullet.ReflectBullet();
            }
        }
    }
    public void DestroyItself()
    {
        Destroy(gameObject);
    }
}
