using System;
using System.Collections;
using UnityEngine;


public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float health;
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
        SetHealth(health-damage);
        if (health <= 0 && !isDead)
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
        if (UnityEngine.Random.value< droppableItems.consumableDropProbability)
        {
            int index = UnityEngine.Random.Range(0, droppableItems.consumableList.Count);
            Instantiate(droppableItems.consumableList[index], transform.position, new Quaternion());
        }
        //TODO:drop weapons
    }
    protected IEnumerator FlashCoroutine()
    {
        spriteRenderer.material = whiteFlashMat;
        yield return new WaitForSeconds(restoreDefaultMatTime);
        spriteRenderer.material = DefaultMat;
    }
}
