using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    public string color;

    private void Start()
    {
        health=maxHealth;
    }
    public void takeDamage(float damage)
    {
        health=Mathf.Clamp(health-damage,0, maxHealth);
        if (health<=0)
        {
            Destroy(GetComponent<EnemyController>().BoundHealthbar);
            Destroy(gameObject);
        }
        
    }
}
