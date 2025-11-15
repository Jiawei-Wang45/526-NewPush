using UnityEngine;

public class EnemySpawnIndicator : MonoBehaviour
{
    public Animator spawnAnimation;

    public EnemyController enemyToSpawn;
    public float animPlaySpeed;

    private void Awake()
    {
        spawnAnimation = GetComponent<Animator>();
        spawnAnimation.speed = animPlaySpeed;
    }
    public void spawnEnemy()
    {
        enemyToSpawn.transform.position = transform.position;
        enemyToSpawn.isAlive(true);  
        Destroy(gameObject);
    }
}
