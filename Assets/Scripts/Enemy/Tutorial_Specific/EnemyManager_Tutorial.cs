using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager_Tutorial : MonoBehaviour
{
    [SerializeField] private List<Transform> EnemySet;
    [SerializeField] private List<GameObject> enemySpawnerIndicators;
    [SerializeField] private float spawnInterval = 0.5f;
    [NonSerialized] private int EnemyCount;

    private void Awake()
    {
        EnemyCount = EnemySet.Count;
    }

    public void SpawnEnemies()
    {
        for (int i=0;i<EnemySet.Count;i++)
        {
            EnemySpawnIndicator_Tutorial enemySpawnIndicator = Instantiate(enemySpawnerIndicators[i], EnemySet[i].position, Quaternion.identity).GetComponent<EnemySpawnIndicator_Tutorial>();
            enemySpawnIndicator.SetIndex(i);
        }
    }
    public void OnEnemyDestroyed(int index)
    {
        StartCoroutine(RespawnEnemyCoroutine(index));
    }
    private IEnumerator RespawnEnemyCoroutine(int index)
    {
        EnemyCount--;
        yield return new WaitForSecondsRealtime(spawnInterval);
        if (EnemyCount==0)
        {
            CleanUp();
        }
        else
        {
            EnemyCount++;
            EnemySpawnIndicator_Tutorial enemySpawnIndicator = Instantiate(enemySpawnerIndicators[index], EnemySet[index].position, Quaternion.identity).GetComponent<EnemySpawnIndicator_Tutorial>();
            enemySpawnIndicator.SetIndex(index);
        }
    }
    private void CleanUp()
    {
        StopAllCoroutines();
        FindFirstObjectByType<AbilityTutorialRoom>().OnAllEnemiesCleared();
    }


}
