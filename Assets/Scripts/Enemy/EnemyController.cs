using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamagable
{

    public PlayerController pc;
    public EnemyStats enemyStats;
    public int challengeLevel;

    public EnemyWeaponData weapon;
    public EnemyMovementPattern movementPattern;
    [NonSerialized] private Rigidbody2D rb;
    [NonSerialized] protected GameManager gameManager;
    [NonSerialized] protected Transform enemyAim;
    public float RotationSpeed = 15.0f;
    public float enemySpeed;
    public float comfortableDistance = 5.0f;

    protected float timeToFire = 0;
    protected bool currentlyFiring = false;
    private bool foundPlayer = false;
    private bool canSeePlayer = false;
    protected LayerMask terrainMask;
    private float checkInterval = 0.1f;
    private Vector3 randomTarget;

    public bool isBoss2 = false;
    private float spinFactor = 1.0f;

    //affected by pause ability
    protected float slowFactor = 1.0f;
    protected Coroutine firingCoroutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStats = GetComponent<EnemyStats>();
        enemyAim = transform.Find("EnemyAim");
        //gameManager.onReset += ResetStates;
    }
    protected virtual void Start()
    {
        pc = PlayerController.instance;
        gameManager = GameManager.instance;

        terrainMask = LayerMask.GetMask("Wall", "Player");
        RefreshStats();
        if (weapon)
            timeToFire = weapon.fireRate - 0.6f;
        randomTarget = transform.position;

        //call back bindings
        PauseManager.instance.OnPauseStart += PauseStart;
        PauseManager.instance.OnPauseEnd += PauseEnd;
        
        isAlive(false);
    }
    public void RefreshStats()
    {
        //enemySpeed = enemyStats.enemyMovementSpeed;
        comfortableDistance = movementPattern.comfortableDistance;
        enemySpeed = movementPattern.MovementSpeed;
    }
    protected virtual void Update()
    {
        if (gameManager.isPlayerAlive)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, pc.transform.position, terrainMask);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer== LayerMask.NameToLayer("Player"))
                {
                    foundPlayer = true;
                    canSeePlayer = true;     
                }
                else
                {
                    canSeePlayer = false;
                    if (movementPattern.LeashTime > 0)
                    {
                        StartCoroutine(LeashPlayer());
                    }
                }
            }
            //else
            //{
            //    foundPlayer = false;
            //}
        }
    }
    protected virtual void FixedUpdate()
    {
        if (gameManager.isPlayerAlive)
        {
            if (foundPlayer)
            {
                Vector2 direction = pc.transform.position - transform.position;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                enemyAim.rotation = Quaternion.Slerp(enemyAim.rotation, targetRotation, RotationSpeed * Time.deltaTime / slowFactor);
                float factor = 1.0f;
                if (canSeePlayer)
                {
                    factor = (transform.position - pc.transform.position).magnitude > comfortableDistance ? 1.0f : -1.0f * movementPattern.BackoffSpeedFactor;
                }
                rb.linearVelocity = factor * direction.normalized * enemySpeed / slowFactor;
            }
            else
            {
                switch (movementPattern.idleBehavior)
                {
                    case EnemyMovementPattern.idleBehaviors.Stops:
                        rb.linearVelocity = Vector2.zero;
                        break;
                    case EnemyMovementPattern.idleBehaviors.RandomWalk:
                        if ((randomTarget - transform.position).magnitude < 0.1f)
                        {
                            randomTarget = transform.position + GetRandomVector3InXY() * 5.0f;
                        }
                        Vector2 direction = randomTarget - transform.position;
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                        enemyAim.rotation = Quaternion.Slerp(enemyAim.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime / slowFactor);
                        rb.linearVelocity = direction.normalized * enemySpeed / slowFactor;
                        break;
                }
            }
        }

        if (canSeePlayer)
        {
            if (!currentlyFiring)
            {
                timeToFire += Time.fixedDeltaTime / slowFactor;
            }
            if (timeToFire >= weapon.fireRate)
            {
                timeToFire = 0;
                firingCoroutine = StartCoroutine(BeginFiringSequence());
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if(movementPattern.idleBehavior == EnemyMovementPattern.idleBehaviors.RandomWalk && !foundPlayer){
                randomTarget = transform.position + GetRandomVector3InXY() * 5.0f;
                RaycastHit2D hit = Physics2D.Linecast(transform.position, randomTarget, terrainMask);
                int tries = 0;
                while(hit.collider != null)
                {
                    randomTarget = transform.position + GetRandomVector3InXY() * 5.0f;
                    hit = Physics2D.Linecast(transform.position, randomTarget, terrainMask);
                    tries++;
                    if(tries > 10){
                        break;
                        // we probably spawned in wall, let the physics engine sort it out
                        // Fixing spawner logic to avoid spawning in walls should fix this
                    }
                    Debug.Log(tries);
                }
            }
        }
    }

    private IEnumerator LeashPlayer()
    {
        float elapsedTime = 0;
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            if (canSeePlayer) break;
            elapsedTime += checkInterval;
            if (elapsedTime> movementPattern.LeashTime)
            {
                foundPlayer = false;
                break;
            }


            //RaycastHit2D hit = Physics2D.Linecast(transform.position, pcTest.transform.position, terrainMask);
            //if (hit.collider != null && hit.collider.transform.position == pcTest.transform.position)
            //{
            //    foundPlayer = true;
            //    canSeePlayer = true;
            //    break;
            //}
            //else
            //{
            //    elapsedTime += checkInterval;
            //    if (elapsedTime >= movementPattern.LeashTime)
            //    {
            //        foundPlayer = false;
            //        canSeePlayer = false;
            //        break;
            //    }
            //}
        }
    }

    private IEnumerator BeginFiringSequence()
    {
        currentlyFiring = true;
        if (!weapon.isComplex)
        {
            yield return FireBulletPattern(weapon.bulletPattern, 0, 0);
        } else
        {
            for(int i = 0; i < weapon.compositePatterns.Length; i++)
            {
                BulletPattern pattern = weapon.compositePatterns[i];
                float angleOffset = weapon.angleOffsets.Length == weapon.compositePatterns.Length ? weapon.angleOffsets[i] : 0;
                float speedOffset = weapon.speedOffsets.Length == weapon.compositePatterns.Length ? weapon.speedOffsets[i] : 0;
                StartCoroutine(FireBulletPattern(pattern, angleOffset, speedOffset));
                yield return new WaitForSeconds(pattern.waitUntilDone ? (pattern.timeBetweenFiring * pattern.fireCount) + pattern.delayAfterPattern : pattern.delayAfterPattern);
            }
        }
        currentlyFiring = false;

    }

    private IEnumerator FireBulletPattern(BulletPattern pattern, float angleOffset, float speedOffset)
    {

        for (int i = 0; i < pattern.fireCount; i++)
        {
            FireOnce(i, pattern, angleOffset, speedOffset);
            if (i < pattern.fireCount - 1)
            {
                yield return new WaitForSeconds(pattern.timeBetweenFiring * slowFactor);
            }
            if (isBoss2)
            {
                spinFactor += 0.06f;
            }
        }
    }

    private void FireOnce(int volleyIndex, BulletPattern pattern, float angleOffset, float speedOffset)
    {
        float baseAngle = enemyAim.eulerAngles.z + (pattern.rotateBetweenFiring * volleyIndex * spinFactor) + pattern.baseAngleOffset + angleOffset;
        float bulletSpeed = pattern.bulletSpeed + speedOffset;
        if (pattern.bulletCount == 1)
        {
            float speedVariance = UnityEngine.Random.Range(pattern.speedVariance, -pattern.speedVariance);
            if (pattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Even)
            {
                CreateBullet(baseAngle, bulletSpeed + speedVariance);
            }
            else
            {
                CreateBullet(baseAngle + UnityEngine.Random.Range(-pattern.firingAngle / 2, pattern.firingAngle / 2), bulletSpeed + speedVariance);
            }
        }
        else
        {
            List<float> angleChanges = new List<float>();
            float angleChange = -pattern.firingAngle / 2;
            float changeStep = pattern.firingAngle / (pattern.bulletCount - 1);
            switch (pattern.bulletDistribution)
            {
                case BulletPattern.bulletDistributionTypes.Even:
                    for (int i = 0; i < pattern.bulletCount; i++)
                    {
                        angleChanges.Add(angleChange);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.SemiRandom:
                    for (int i = 0; i < pattern.bulletCount; i++)
                    {
                        float randomAdjustment = UnityEngine.Random.Range(-changeStep / 3, changeStep / 3);
                        angleChanges.Add(angleChange + randomAdjustment);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Random:
                    for (int i = 0; i < pattern.bulletCount; i++)
                    {
                        angleChange = UnityEngine.Random.Range(-pattern.firingAngle / 2, pattern.firingAngle / 2);
                        angleChanges.Add(angleChange);
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Radial:
                    for (int i = 0; i < pattern.bulletCount; i++)
                    {
                        angleChange = 0 + (pattern.firingAngle/pattern.bulletCount * i);
                        angleChanges.Add(angleChange);
                    }
                    break;
            }
            int bulletInd = 0;
            Vector3 centerPosition = isBoss2 ? GetRandomVector3InXY() : Vector3.zero;
            foreach (float change in angleChanges)
            {
                float speedVariance = UnityEngine.Random.Range(pattern.speedVariance, -pattern.speedVariance);
                if (pattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Radial)
                {
                    CreateBullet(
                        change + (volleyIndex * pattern.rotateBetweenFiring * spinFactor), 
                        bulletSpeed + (bulletInd * weapon.bulletSpeedRange / pattern.bulletCount) + speedVariance);
                }
                else
                {
                    CreateBullet(
                        baseAngle + change + (volleyIndex * pattern.rotateBetweenFiring * spinFactor),
                        bulletSpeed + (bulletInd * weapon.bulletSpeedRange / pattern.bulletCount) + speedVariance, 
                        0.0f, 
                        centerPosition);   
                }
                bulletInd++;
            }   
        }
    }

    private void CreateBullet(float angle, float speed, float offsetDistance = 0.0f, Vector3? centerPosition = null)
    {
        if(centerPosition == null) centerPosition = Vector3.zero;
        Vector2 spawnVector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 spawnPosition = Vector3.zero;
        offsetDistance = GetComponent<Collider2D>().bounds.extents.magnitude * 0.9f;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        if(!weapon.isLaser)
        {
            spawnPosition = transform.position + (Vector3)centerPosition + (Vector3)(spawnVector * offsetDistance);
            GameObject spawnedBullet = Instantiate(weapon.bulletType, spawnPosition, rotation);
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();   
            bulletAttributes.InitBullet(speed, weapon.bulletDamage);
            if(PauseManager.instance.isPausing)
            {
                bulletAttributes.PauseStart(slowFactor);
            }
        }
        else
        {
            spawnPosition = transform.position + GetRandomVector3InXY() * 2.0f + (Vector3)(spawnVector * offsetDistance);
            Vector2 direction = pc.transform.position - spawnPosition;
            float laserAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(laserAngle, Vector3.forward);
            Quaternion laserRotation = targetRotation * Quaternion.Euler(0.0f, 0.0f, 90.0f);
                    
            GameObject spawnedLaser = Instantiate(weapon.bulletType, spawnPosition - laserRotation * Vector2.up * 50.0f, laserRotation);
            Bullet_Laser laserAttributes = spawnedLaser.GetComponent<Bullet_Laser>();
            laserAttributes.InitBulletwithSpinup(0.0f, weapon.bulletDamage, 0, 2.0f);
        }
    }

    public void isAlive(bool status)
    {
        gameObject.SetActive(status);
    }

    //public void ResetStates()
    //{
    //    StopAllCoroutines();
    //    enemyStats.Reset();
    //    timeToFire = weapon.fireRate - 0.6f;
    //    isAlive(false);
    //}
    public void PauseStart(float pauseStrength)
    {
        slowFactor=pauseStrength;

    }
    public void PauseEnd()
    {
        slowFactor = 1;
    }


    public void TakeDamage(float damage)
    {
        enemyStats.TakeDamage(damage);
    }
    private Vector3 GetRandomVector3InXY()
    {
        float angle=UnityEngine.Random.Range(0, 2 * Mathf.PI);
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle),0);
    }
    protected virtual void OnDestroy()
    {
        PauseManager.instance.OnPauseStart -= PauseStart;
        PauseManager.instance.OnPauseEnd -= PauseEnd;
        //gameManager.onReset -= ResetStates;
    }

}
