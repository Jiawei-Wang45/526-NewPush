using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    private EnemySpawner spawner;

    public HSLColor enemyColor = new HSLColor();


    private void Start()
    {
        health = maxHealth;

        enemyColor = new HSLColor(200f, 100f, 50f);

        spawner = FindFirstObjectByType<EnemySpawner>();
    }
    public void takeDamage(float damage)
    {
        health -= damage;
        enemyColor.L = 50f + (health / maxHealth) * 40f;
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
