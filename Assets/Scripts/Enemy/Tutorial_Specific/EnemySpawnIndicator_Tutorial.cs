using System;
using UnityEngine;

public class EnemySpawnIndicator_Tutorial : MonoBehaviour
{
    public Animator spawnAnimation;

    public GameObject enemyToSpawn;

    public float animPlaySpeed;
    [SerializeField] private EnemyStats_Tutorial.SceneType type;
    [NonSerialized] private int index;
    private void Awake()
    {
        spawnAnimation = GetComponent<Animator>();
        spawnAnimation.speed = animPlaySpeed;
    }

    public void spawnEnemy()
    {
        GameObject spawnedEnemy=Instantiate(enemyToSpawn, transform.position, Quaternion.identity);
        EnemyStats_Tutorial enemyState = spawnedEnemy.GetComponent<EnemyStats_Tutorial>();
        enemyState.type = type;
        enemyState.index = index;
        Destroy(gameObject);
    }
    public void SetIndex(int inIndex)
    {
        index = inIndex;
    }
}
