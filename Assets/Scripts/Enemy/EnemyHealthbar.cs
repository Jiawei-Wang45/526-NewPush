using UnityEngine;

public class EnemyHealthbar : MonoBehaviour
{
    public UnityEngine.UI.Image healthbarImage;
    public GameObject enemy;
    private EnemyStats enemyStats;
    public float offsetX;
    public float offsetY;

    private void Start()
    {
        healthbarImage=GetComponent<UnityEngine.UI.Image>();
        enemyStats = enemy.GetComponent<EnemyStats>();
        offsetY = -enemy.GetComponent<SpriteRenderer>().bounds.size.y / 2-0.2f;
    }
    private void Update()
    {
        healthbarImage.fillAmount = enemyStats.health / enemyStats.maxHealth;
    }
    private void LateUpdate()
    {
        transform.position = enemy.transform.position + new Vector3(offsetX, offsetY, 0);
    }
}
