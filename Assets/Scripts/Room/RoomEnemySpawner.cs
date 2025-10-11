using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyGroup
    {
        public GameObject enemyPrefab;     // 敌人类型
        public int amount;                 // 该敌人生成数量
        public float delayBetweenSpawns;   // 该类敌人生成间隔
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";   // 方便调试或UI显示
        public List<EnemyGroup> enemyGroups = new List<EnemyGroup>();
        public float delayAfterWave = 1.5f; // 该波结束后等待时间
    }

    public List<Wave> waves = new List<Wave>();           // 所有波次配置
    public GameObject enemySpawnIndicatorPrefab;          // 指示器预制体
    public List<Transform> spawnPoints = new List<Transform>(); // 多个生成点

    private int currentEnemies = 0;
    private bool allWavesDone = false;

    public bool IsAllWavesCleared => allWavesDone;

    public void StartSpawning()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        foreach (var wave in waves)
        {
            Debug.Log($"Start spawning wave: {wave.waveName}");

            // 遍历每种敌人类型
            foreach (var group in wave.enemyGroups)
            {
                // 启动子协程生成同类敌人（异步）
                StartCoroutine(SpawnEnemyGroup(group));
                yield return new WaitForSeconds(0.2f); // 稍微错开不同组的开始
            }

            // 等待当前波敌人全部死亡
            yield return new WaitUntil(() => currentEnemies <= 0);

            // 等待波与波之间的间隔
            yield return new WaitForSeconds(wave.delayAfterWave);
        }

        allWavesDone = true;
    }

    private IEnumerator SpawnEnemyGroup(EnemyGroup group)
    {
        for (int i = 0; i < group.amount; i++)
        {
            // 随机选择一个生成点
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Count)];
            Vector3 spawnPos = point.position;

            // 生成指示器
            GameObject indicator = Instantiate(enemySpawnIndicatorPrefab, spawnPos, Quaternion.identity);
            RoomEnemySpawnIndicator indicatorScript = indicator.GetComponent<RoomEnemySpawnIndicator>();
            indicatorScript.enemyToSpawn = group.enemyPrefab;
            indicatorScript.spawner = this;

            currentEnemies++;

            yield return new WaitForSeconds(group.delayBetweenSpawns);
        }
    }

    public void RegisterEnemy(GameObject enemy)
    {
        StartCoroutine(WaitForDeath(enemy));
    }

    private IEnumerator WaitForDeath(GameObject enemy)
    {
        while (enemy != null)
        {
            yield return null;
        }
        currentEnemies = Mathf.Max(0, currentEnemies - 1);
    }

    public bool IsRoomCleared()
    {
        return currentEnemies == 0;
    }
}
