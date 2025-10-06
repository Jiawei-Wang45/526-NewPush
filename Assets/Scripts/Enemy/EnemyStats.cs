using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;

    private void Start()
    {
        health=maxHealth;
    }
    public void takeDamage(float damage)
    {
        health-=damage; 
        if (health<=0)
        {
            Destroy(GetComponent<EnemyController>().BoundHealthbar);
            Destroy(gameObject);
        }
    }
}
