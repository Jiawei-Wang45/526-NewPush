using UnityEngine;

public class EnemySpawnIndicator : MonoBehaviour
{
    public Animator spawnAnimation;
    public GameObject enemyToSpawn;
    public GameObject enemyBoundHealthbar;
    public float animPlaySpeed;

    private void Start()
    {
        spawnAnimation=GetComponent<Animator>();
        spawnAnimation.speed = animPlaySpeed;
    }
    public void spawnEnemy()
    {
        GameObject spawnedEnemy=Instantiate(enemyToSpawn,transform.position,transform.rotation);
        GameObject createdHealthbar=Instantiate(enemyBoundHealthbar, transform.position, transform.rotation);

        // initialize reference in both sides
        spawnedEnemy.GetComponent<EnemyController>().BoundHealthbar = createdHealthbar;
        createdHealthbar.GetComponentInChildren<EnemyHealthbar>().enemy=spawnedEnemy;
        Destroy(gameObject);
    }
}
