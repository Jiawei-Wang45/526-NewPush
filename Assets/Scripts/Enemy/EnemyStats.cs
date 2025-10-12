using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    public string color;
    
    private EnemyHSLSystem hslSystem;

    private void Start()
    {
        hslSystem = GetComponent<EnemyHSLSystem>();
        
        // 如果HSL系统存在，让HSL系统管理血量
        if (hslSystem != null)
        {
            hslSystem.currentHealth = maxHealth;
            hslSystem.maxHealth = maxHealth;
            health = hslSystem.currentHealth; // 从HSL系统同步
        }
        else
        {
            health = maxHealth; // 如果没有HSL系统，使用默认值
        }
    }
    
    private void Update()
    {
        // 确保血量始终与HSL系统同步
        if (hslSystem != null)
        {
            health = hslSystem.currentHealth;
        }
    }
    
    public void takeDamage(float damage)
    {
        // 如果HSL系统存在，让HSL系统处理伤害
        if (hslSystem != null)
        {
            hslSystem.TakeDamage(damage);
            health = hslSystem.currentHealth; // 从HSL系统同步血量
        }
        else
        {
            health = Mathf.Clamp(health - damage, 0, maxHealth);
        }
        
        if (health <= 0)
        {
            Destroy(GetComponent<EnemyController>().BoundHealthbar);
            Destroy(gameObject);
        }
    }
    
    public void Heal(float healAmount)
    {
        // 如果HSL系统存在，让HSL系统处理治疗
        if (hslSystem != null)
        {
            hslSystem.Heal(healAmount);
            health = hslSystem.currentHealth; // 从HSL系统同步血量
        }
        else
        {
            health += healAmount;
            health = Mathf.Clamp(health, 0f, maxHealth);
        }
    }
}
