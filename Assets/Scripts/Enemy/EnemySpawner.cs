using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval;
    public float spawnAmount;

    public GameObject[] enemySpawnList;
    public GameObject enemySpawnIndicator;

    private float defaultSpawnPosX = 4;
    private float defaultSpawnPosY = 4;

    private float spawnPosX;
    private float spawnPosY;

    public float spawnPosVariationScale;

    // Random seed for enemy spawn positions, generated per game session
    private int seed; 

    private void Start()
    {
        // Generate a random seed for this game session to randomize enemy positions across games
        seed = Random.Range(0, 1000000);
        // Initialize random state with the seed for consistent positions within the session
        Random.InitState(seed); 
        InitspawnPos();
        StartCoroutine(EnemySpawnProcess());
    }
    private IEnumerator EnemySpawnProcess()
    {
        for (int index=0;index<enemySpawnList.Length;index++)
        {
            yield return new WaitForSeconds(spawnInterval);
            for (int amount=0;amount<spawnAmount;amount++)
            {
                GameObject spawnedIndicator=Instantiate(enemySpawnIndicator, transform.position+new Vector3(Random.Range(-spawnPosX, spawnPosX),Random.Range(-spawnPosY, spawnPosY)), transform.rotation);
                spawnedIndicator.GetComponent<EnemySpawnIndicator>().enemyToSpawn = enemySpawnList[index];
            }
        }
        FindFirstObjectByType<GameManager>().SetSpawnCompleted();
    }
    public void ResetSpawner()
    {
        StopAllCoroutines();
        // Reset random state to the same seed for consistent enemy positions during ghost replay
        Random.InitState(seed); 
        StartCoroutine(EnemySpawnProcess());
    }
    public void InitspawnPos()
    {
        spawnPosX = defaultSpawnPosX * spawnPosVariationScale;
        spawnPosY = defaultSpawnPosY * spawnPosVariationScale;
    }

}
