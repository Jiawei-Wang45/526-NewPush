using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    private EnemySpawner spawner;
    private void Start()
    {
        health = maxHealth;
        spawner = FindFirstObjectByType<EnemySpawner>();
    }
    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GetComponent<EnemyController>().isAlive(false);
            spawner.EnemyDestroyed();
        }
    }
    
    public void Reset()
    {
        health = maxHealth;
    }
}
