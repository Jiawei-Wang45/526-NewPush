using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class EnemySpawner : MonoBehaviour
{
    public GameObject[] enemySpawnList;
    public int[] canSpawnAfterRoomNumber;
    //Optional parameter that can let the user set enemies to spawn only after a certain number of rooms
    private List<GameObject> roomSpawnList = new List<GameObject>();
    private List<int> difficultyCosts = new List<int>();

    public GameObject enemySpawnIndicator;
    public GameManager gameManager;
    public GameObject spawnBox;
    public int spawnBudget = 20;
    // Optional fixed per-wave budgets. If provided and length > 0, the spawner will use these budgets per wave.
    public int[] waveBudgets;
    // If true, the spawner will automatically start the next wave after a clear (or when totalWaves is 0 meaning infinite).
    public bool loopWaves = true;
    // If > 0, the spawner will run this many waves and then finish. If 0, waves are infinite depending on loopWaves.
    public int totalWaves = 1;

    private List<EnemyController> enemiesInWave = new List<EnemyController>();
    private List<Vector2> spawnPositions = new List<Vector2>();

    private int enemiesStillAlive;

    // Wave state exposed to other scripts
    public bool IsWaveActive { get; private set; } = false;
    public bool IsWaveCleared { get; private set; } = false;
    public int currentWaveIndex = 0;

    // Event fired when current wave is cleared
    public event Action OnWaveCleared;
    // Event fired when the spawner has finished all its waves (only if it has finite waves)
    public event Action OnSpawnerFinished;
    private void Awake()
    {
        for (int i = 0; i < enemySpawnList.Length; i++)
        {
            EnemyController ec = enemySpawnList[i].GetComponent<EnemyController>();
            difficultyCosts.Add(ec.challengeLevel);
        }
    }

    public void InitializeNewWave()
    {
        roomSpawnList.Clear();
        difficultyCosts.Clear();
        Debug.Log($"Cleared room count: {gameManager.clearedRoomCount}");
        for (int i = 0; i < enemySpawnList.Length; i++)
        {
            if(enemySpawnList.Length != canSpawnAfterRoomNumber.Length || canSpawnAfterRoomNumber[i] <= gameManager.clearedRoomCount)
            {
                EnemyController ec = enemySpawnList[i].GetComponent<EnemyController>();
                roomSpawnList.Add(enemySpawnList[i]);
                difficultyCosts.Add(ec.challengeLevel);
            }
        }
        int pointsToSpend = spawnBudget;
        // If fixed wave budgets are provided, use the budget for the current wave index
        if (waveBudgets != null && waveBudgets.Length > 0)
        {
            int idx = Mathf.Clamp(currentWaveIndex, 0, waveBudgets.Length - 1);
            pointsToSpend = waveBudgets[idx];
        }
        Debug.Log($"{roomSpawnList.Count} vs {difficultyCosts.Count}");
        while (pointsToSpend > 0)
        {
            int index = UnityEngine.Random.Range(0, roomSpawnList.Count);
            pointsToSpend -= difficultyCosts[index];
            enemiesInWave.Add(InstantiateNewEnemy(roomSpawnList[index]));
            spawnPositions.Add(GetRandomSpawnPoint());
        }
    }

    public void SpawnWave()
    {
        enemiesStillAlive = enemiesInWave.Count;
        Debug.Log($"EnemySpawner '{name}': Spawning wave {currentWaveIndex + 1} with {enemiesStillAlive} enemies.");
        IsWaveActive = true;
        IsWaveCleared = false;
        for (int i = 0; i < enemiesInWave.Count; i++)
        {
            GameObject spawnedIndicator = Instantiate(enemySpawnIndicator, spawnPositions[i], transform.rotation);
            spawnedIndicator.GetComponent<EnemySpawnIndicator>().enemyToSpawn = enemiesInWave[i];
        }
    }
    
    public void EnemyDestroyed()
    {
        enemiesStillAlive--;
        if (enemiesStillAlive == 0)
        {
            WaveCleared();
        }
    }

    private Vector2 GetRandomSpawnPoint()
    {
        SpriteRenderer sr = spawnBox.GetComponent<SpriteRenderer>();
        Bounds b = sr.bounds;

        float x = UnityEngine.Random.Range(b.min.x, b.max.x);
        float y = UnityEngine.Random.Range(b.min.y, b.max.y);
        return new Vector2(x, y);
    }

    public void StartWave()
    {
        // If totalWaves > 0 and we've already reached the total, do nothing (finished)
        if (totalWaves > 0 && currentWaveIndex >= totalWaves)
        {
            IsWaveActive = false;
            IsWaveCleared = true;
            OnSpawnerFinished?.Invoke();
            return;
        }

        InitializeNewWave();
        SpawnWave();
    }

    private void WaveCleared()
    {
        spawnPositions.Clear();
        foreach (EnemyController e in enemiesInWave)
        {
            Destroy(e.gameObject);
        }
        enemiesInWave.Clear();
        // mark state
        IsWaveActive = false;
        IsWaveCleared = true;

        // Notify listeners that this wave was cleared
        OnWaveCleared?.Invoke();

        // GameManager no longer manages waves; RoomManager/other listeners
        // should subscribe to OnWaveCleared/OnSpawnerFinished as needed.

        // Advance to next wave index
        currentWaveIndex++;

        // If we have a finite number of waves and we've reached the end, signal finished
        if (totalWaves > 0 && currentWaveIndex >= totalWaves)
        {
            OnSpawnerFinished?.Invoke();
            IsWaveActive = false;
            IsWaveCleared = true;
            return;
        }

        // Otherwise start next wave if looping or totalWaves is 0 (infinite) or still have remaining finite waves
        if (loopWaves || (totalWaves > 0 && currentWaveIndex < totalWaves))
        {
            // If using waveBudgets, wrap/wrapless handled by InitializeNewWave via currentWaveIndex
            StartWave();
        }
        
    }

    // Returns true if this spawner has a finite number of waves that will eventually finish.
    public bool HasFiniteWaves()
    {
        return totalWaves > 0;
    }

    // Returns true when the spawner has finished all its waves (only meaningful if HasFiniteWaves==true)
    public bool HasFinishedAllWaves()
    {
        if (!HasFiniteWaves()) return false;
        return currentWaveIndex >= totalWaves && !IsWaveActive;
    }


    private EnemyController InstantiateNewEnemy(GameObject enemy)
    {
        GameObject spawnedEnemy=Instantiate(enemy,transform.position,transform.rotation);
        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
        // Ensure the spawned enemy knows which spawner created it so callbacks go to the correct spawner
        var stats = spawnedEnemy.GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.spawner = this;
        }
        return enemyController;
    }
}

