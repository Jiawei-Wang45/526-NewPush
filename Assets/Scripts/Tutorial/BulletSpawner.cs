using System;
using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public enum SpawnerType
    {
        ShortInterval,
        LongInterval
    }
    [SerializeField] private GameObject bulletType;
    [NonSerialized] private float ShootingInterval;
    [SerializeField] public SpawnerType spawnerType;
    [NonSerialized] public bool isShooting;

    private void Start()
    {

        if (spawnerType == SpawnerType.ShortInterval)
        {
            ShootingInterval = 0.2f;
            FindFirstObjectByType<AbilityTutorialRoom>().RegisterBulletSpawner(this);
        }
        else
        {
            ShootingInterval = 2f;
            FindFirstObjectByType<DefenseTutorialRoom>().RegisterBulletSpawner(this);
        }

    }
    public void SpawnBullet()
    {
        GameObject spawnedBullet = Instantiate(bulletType, transform.position, Quaternion.identity);
        //Bullet_Tutorial bulletAttributes = spawnedBullet.GetComponent<Bullet_Tutorial>();
    }
    public void StartEndlessShooting()
    {
        StartCoroutine(EndlessShootingCoRoutine());
    }
    public IEnumerator EndlessShootingCoRoutine()
    {
        isShooting = true;
        while (isShooting)
        {
            SpawnBullet();
            yield return new WaitForSeconds(ShootingInterval);
        }

    }
}
