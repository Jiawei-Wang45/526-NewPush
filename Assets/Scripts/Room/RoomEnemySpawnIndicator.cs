using UnityEngine;
using System;

public class RoomEnemySpawnIndicator : MonoBehaviour
{
    public GameObject enemyToSpawn;
    public GameObject enemyBoundHealthbar;

    [HideInInspector] public RoomEnemySpawner spawner;

    private void Start()
    {
        // 直接生成敌人，不需要动画
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        // 生成敌人
        GameObject spawnedEnemy = Instantiate(enemyToSpawn, transform.position, transform.rotation);
        GameObject createdHealthbar = Instantiate(enemyBoundHealthbar, transform.position, transform.rotation);

        // 绑定血条
        var enemyController = spawnedEnemy.GetComponent<EnemyController>();
        if (enemyController != null)
            enemyController.BoundHealthbar = createdHealthbar;

        var healthbar = createdHealthbar.GetComponentInChildren<EnemyHealthbar>();
        if (healthbar != null)
            healthbar.enemy = spawnedEnemy;

        // 注册到 spawner
        if (spawner != null)
        {
            spawner.RegisterEnemy(spawnedEnemy);
        }

        // 销毁指示器
        Destroy(gameObject);
    }
}
