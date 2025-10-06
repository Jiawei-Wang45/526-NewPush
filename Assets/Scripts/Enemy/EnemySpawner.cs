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

    private void Start()
    {
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
    }
    public void InitspawnPos()
    {
        spawnPosX = defaultSpawnPosX * spawnPosVariationScale;
        spawnPosY=defaultSpawnPosY* spawnPosVariationScale;
    }

}
