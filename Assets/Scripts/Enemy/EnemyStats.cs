using System;
using System.Collections;
using UnityEngine;


public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float health;
    public int life = 0;
    public float enemyMovementSpeed;   
    public EnemySpawner spawner;


    public delegate void HealthChangedDelegate();
    public event HealthChangedDelegate OnHealthChanged;
    public DroppableItems droppableItems;
    //particle effect
    public GameObject dyingEffect;
    //Flash effect
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = 0.2f;
    [NonSerialized] private SpriteRenderer spriteRenderer;
    [NonSerialized] private Material DefaultMat;
    [NonSerialized] private bool isDead;
    protected virtual void Awake()
    {
        health = maxHealth;
        isDead = false;
        spriteRenderer =GetComponent<SpriteRenderer>();
        DefaultMat = spriteRenderer.material;
    }
    private void Start()
    {
        
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
    }
    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;
        SetHealth(health-damage);
        if (health <= 0)
        {
            if(life > 0)
            {
                life--;
                SetHealth(maxHealth);
                StartCoroutine(FlashCoroutine());
            }
            else
            {
                RandomDropItems();

                GameObject particle=Instantiate(dyingEffect, transform.position, Quaternion.identity);

                GetComponent<EnemyController>().isAlive(false);
                isDead = true;
                if (spawner)
                {
                    spawner.EnemyDestroyed();
                }   
            }
        }
        else
        {
            StartCoroutine(FlashCoroutine());
        }
    }
    public void SetHealth(float newHealth)
    {
        health = newHealth;
        OnHealthChanged?.Invoke();
    }
    //public void Reset()

    //{
    //    SetHealth(maxHealth);
    //    OnHealthChanged?.Invoke();
    //}
    protected void RandomDropItems()
    {
        if (!droppableItems) return;
        GameObject droppedItem = droppableItems.DropItem();
        if (droppedItem != null)
        {
            Instantiate(droppedItem, transform.position, new Quaternion());
        }
    }
    protected IEnumerator FlashCoroutine()
    {
        spriteRenderer.material = whiteFlashMat;
        yield return new WaitForSeconds(restoreDefaultMatTime);
        spriteRenderer.material = DefaultMat;
    }
}
