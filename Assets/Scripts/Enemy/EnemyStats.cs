using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    private EnemySpawner spawner;

    public HSLColor enemyColor = new HSLColor();
    public delegate void HealthChangedDelegate();
    public event HealthChangedDelegate OnHealthChanged;
    public DroppableItems droppableItems;
    private void Start()
    {
        health = maxHealth;

        enemyColor = new HSLColor(200f, 100f, 50f);

        spawner = FindFirstObjectByType<EnemySpawner>();
    }
    public void TakeDamage(float damage)
    {
        SetHealth(health-damage);
        enemyColor.L = 50f + (health / maxHealth) * 40f;
        if (health <= 0)
        {
            RandomDropItems();
            GetComponent<EnemyController>().isAlive(false);
            spawner.EnemyDestroyed();
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
}
