using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval;
    public float spawnAmount;

    public GameObject[] enemySpawnList;
    private List<int> difficultyCosts = new List<int>();

    public GameObject enemySpawnIndicator;
    public GameObject enemyHealthBar;
    public GameManager gameManager;
    public GameObject spawnBox;
    public int spawnBudget = 20;
    private List<EnemyController> enemiesInWave = new List<EnemyController>();
    private List<Vector2> spawnPositions = new List<Vector2>();

    private int enemiesStillAlive;
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
        int pointsToSpend = spawnBudget;
        List<(int cost, int index)> costIndexList = new List<(int, int)>();
        for (int i = 0; i < difficultyCosts.Count; i++)
        {
            costIndexList.Add((difficultyCosts[i], i));
        }
        costIndexList = costIndexList.OrderByDescending(t => t.Item1).ToList();
        while (pointsToSpend > 0)
        {
            (int, int) chosenIndex = costIndexList[Random.Range(0, costIndexList.Count)];
            pointsToSpend -= chosenIndex.Item1;
            enemiesInWave.Add(InstantiateNewEnemy(enemySpawnList[chosenIndex.Item2]));
            spawnPositions.Add(GetRandomSpawnPoint());
            while (pointsToSpend < costIndexList[0].Item1 && costIndexList.Count > 0 && pointsToSpend > 0)
            {
                costIndexList.RemoveAt(0);
            }
            if (costIndexList.Count == 0)
            {
                break;
            }
        }
    }

    public void SpawnWave()
    {
        enemiesStillAlive = enemiesInWave.Count;
        for (int i = 0; i < enemiesInWave.Count; i++)
        {
            GameObject spawnedIndicator = Instantiate(enemySpawnIndicator, spawnPositions[i], transform.rotation);
            spawnedIndicator.GetComponent<EnemySpawnIndicator>().enemyToSpawn = enemiesInWave[i];
        }
    }
    
    public void EnemyDestroyed()
    {

        enemiesStillAlive--;
        Debug.Log($"Enemy destroyed {enemiesStillAlive} left");
        if (enemiesStillAlive == 0)
        {
            WaveCleared();
        }
    }
    
    private Vector2 GetRandomSpawnPoint()
    {
        SpriteRenderer sr = spawnBox.GetComponent<SpriteRenderer>();
        Bounds b = sr.bounds;

        float x = Random.Range(b.min.x, b.max.x);
        float y = Random.Range(b.min.y, b.max.y);
        return new Vector2(x, y);
    }

    private void Start()
    {
        StartNewWave();
    }

    private void WaveCleared()
    {
        spawnPositions.Clear();
        foreach (EnemyController e in enemiesInWave)
        {
            e.Erase();
        }
        enemiesInWave.Clear();
        spawnBudget += 10;
        gameManager.WaveClear();
    }

    public void StartNewWave()
    {
        InitializeNewWave();
        SpawnWave();
    }


    private EnemyController InstantiateNewEnemy(GameObject enemy)
    {
        GameObject spawnedEnemy=Instantiate(enemy,transform.position,transform.rotation);
        GameObject createdHealthbar = Instantiate(enemyHealthBar, transform.position, transform.rotation);
        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();
        spawnedEnemy.GetComponent<EnemyController>().BoundHealthbar = createdHealthbar;
        createdHealthbar.GetComponentInChildren<EnemyHealthbar>().enemy = spawnedEnemy;
        return enemyController;
    }
}

