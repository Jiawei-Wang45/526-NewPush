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
}
