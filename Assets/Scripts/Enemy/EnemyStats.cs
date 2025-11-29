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
    [SerializeField] private float restoreDefaultMatTime = 0.2f;
    [NonSerialized] private SpriteRenderer spriteRenderer;
    [NonSerialized] private bool isDead;
    [NonSerialized] private EnemyHealthbar healthbar;
    [NonSerialized] private Animator anim;
    protected virtual void Awake()
    {
        health = maxHealth;
        isDead = false;
        spriteRenderer =GetComponent<SpriteRenderer>();
        healthbar = GetComponentInChildren<EnemyHealthbar>();
        anim = GetComponent<Animator>();
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
                
                // Trigger death animation if animator exists
                if (anim != null)
                {
                    anim.SetTrigger("Die");
                }
                
                // Hide healthbar and healthbar background on death
                if (healthbar != null)
                {
                    healthbar.gameObject.SetActive(false);
                }
                
                // Find and hide HealthBarBG
                Transform healthBarBG = transform.Find("HealthBarBG");
                if (healthBarBG != null)
                {
                    healthBarBG.gameObject.SetActive(false);
                }

                isDead = true;
                
                // Stop movement immediately during death animation
                Rigidbody2D rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                
                // Disable controller to stop AI behavior during death
                EnemyController controller = GetComponent<EnemyController>();
                if (controller != null)
                {
                    controller.enabled = false;
                }
                
                // Delay disabling enemy to allow death animation to play (0.7 seconds)
                StartCoroutine(DeathSequence());
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
        // Save original color
        Color originalColor = spriteRenderer.color;
        
        // Brighten the sprite by multiplying RGB values while preserving alpha
        spriteRenderer.color = new Color(
            Mathf.Min(originalColor.r * 1.8f, 1f),
            Mathf.Min(originalColor.g * 1.8f, 1f),
            Mathf.Min(originalColor.b * 1.8f, 1f),
            originalColor.a
        );
        
        yield return new WaitForSeconds(restoreDefaultMatTime);
        
        // Restore original color
        spriteRenderer.color = originalColor;
    }
    
    protected IEnumerator DeathSequence()
    {
        // Wait for death animation to finish (0.7 seconds)
        yield return new WaitForSeconds(0.7f);
        
        // Disable enemy after animation completes
        GetComponent<EnemyController>().isAlive(false);
        
        if (spawner)
        {
            spawner.EnemyDestroyed();
        }
    }
}
