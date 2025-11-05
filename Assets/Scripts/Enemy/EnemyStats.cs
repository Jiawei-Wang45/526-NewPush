using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    //public float enemyDamage;
    public float health;
    public EnemySpawner spawner;

    public HSLColor enemyColor = new HSLColor();
    public delegate void HealthChangedDelegate();
    public event HealthChangedDelegate OnHealthChanged;
    public DroppableItems droppableItems;
    //particle effect
    public GameObject dyingEffect;
    //Flash effect
    [SerializeField] private Material whiteFlashMat;
    [SerializeField] private float restoreDefaultMatTime = 0.2f;
    private SpriteRenderer spriteRenderer;
    private Material DefaultMat;
    private void Awake()
    {
        health = maxHealth;
        enemyColor = new HSLColor(200f, 100f, 50f);
        spriteRenderer=GetComponent<SpriteRenderer>();
        DefaultMat = spriteRenderer.material;
    }
    private void Start()
    {
        
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<EnemySpawner>();
        }
    }
    public void TakeDamage(float damage)
    {
        SetHealth(health-damage);
        enemyColor.L = 50f + (1 - (health / maxHealth)) * 25;
        
        if (health <= 0)
        {
            RandomDropItems();

            GameObject particle=Instantiate(dyingEffect, transform.position, new Quaternion());

            var main = particle.GetComponent<ParticleSystem>().main;
            main.startColor = enemyColor.ToRGB();

            GetComponent<EnemyController>().isAlive(false);
            spawner.EnemyDestroyed();
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
    public void Reset()
    
    {
        SetHealth(maxHealth);
        OnHealthChanged?.Invoke();
    }
    private void RandomDropItems()
    {
        if (!droppableItems) return;
        if (Random.value< droppableItems.consumableDropProbability)
        {
            int index = Random.Range(0, droppableItems.consumableList.Count);
            Instantiate(droppableItems.consumableList[index], transform.position, new Quaternion());
        }
        //TODO:drop weapons
    }
    private IEnumerator FlashCoroutine()
    {
        spriteRenderer.material = whiteFlashMat;
        yield return new WaitForSeconds(restoreDefaultMatTime);
        spriteRenderer.material = DefaultMat;
    }
}
