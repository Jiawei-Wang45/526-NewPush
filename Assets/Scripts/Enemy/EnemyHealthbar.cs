using UnityEngine;

public class EnemyHealthbar : MonoBehaviour
{
    public UnityEngine.UI.Image healthbarImage;
    public GameObject enemy;
    private EnemyStats enemyStats;
    private EnemyHSLSystem hslSystem;
    public float offsetX;
    public float offsetY;

    private void Start()
    {
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
        enemyStats = enemy.GetComponent<EnemyStats>();
        hslSystem = enemy.GetComponent<EnemyHSLSystem>();
        offsetY = -enemy.GetComponent<SpriteRenderer>().bounds.size.y / 2 - 0.2f;
    }
    
    private void Update()
    {
        // 更新血条填充量
        healthbarImage.fillAmount = enemyStats.health / enemyStats.maxHealth;
        
        // 如果HSL系统存在，让HSL系统处理颜色更新
        if (hslSystem != null)
        {
            // HSL系统会自动更新颜色，这里不需要额外处理
            // 但我们可以确保healthbarImage被正确引用
            if (hslSystem.healthbarImage == null)
            {
                hslSystem.healthbarImage = healthbarImage;
            }
        }
    }
    
    private void LateUpdate()
    {
        transform.position = enemy.transform.position + new Vector3(offsetX, offsetY, 0);
    }
}
