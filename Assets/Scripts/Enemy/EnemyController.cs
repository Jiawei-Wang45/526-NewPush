using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public PlayerControllerTest pcTest;
    public EnemyStats enemyStats;
    public int challengeLevel;

    public EnemyWeaponData weapon;
    public EnemyMovementPattern movementPattern;
    public Rigidbody2D rb;
    public GameManager gameManager;
    public GameObject BoundHealthbar;
    public float RotationSpeed = 15.0f;
    public float enemySpeed;
    public float comfortableDistance = 5.0f;


    private Vector2 movement;
    private float timeToFire = 0;
    private bool currentlyFiring = false;
    private bool foundPlayer = false;
    private bool canSeePlayer = false;
    private LayerMask terrainMask;
    private float checkInterval = 0.1f;
    private Vector3 randomTarget;
    private List<ObjectState> recordedStates = new List<ObjectState>();
    private int stateIndex = 0;
    private bool isReplayingActions = false;


    public bool isAttacking;

    private bool isPaused = false;
    private Vector2 savedVelocity;
    private float slowFactor = 1.0f;
    private float timeintoslow = 0.0f;

    private void Start()
    {
        pcTest = FindFirstObjectByType<PlayerControllerTest>();
        enemyStats = GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        terrainMask = LayerMask.GetMask("Wall", "Player");
        gameManager = FindFirstObjectByType<GameManager>();
        RefreshStats();
        timeToFire = weapon.fireRate - 0.6f;
        randomTarget = transform.position;
        isAlive(false);
    }
    public void RefreshStats()
    {
        //enemySpeed = enemyStats.enemyMovementSpeed;
        comfortableDistance = movementPattern.comfortableDistance;
        enemySpeed = movementPattern.MovementSpeed;
    }
    private void Update()
    {
        if (isPaused) return;
        UpdateEnemyColor();
        if (!isReplayingActions && gameManager.isPlayerAlive)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, pcTest.transform.position, terrainMask);
            if (hit.collider != null)
            {
                if (hit.collider.transform.position == pcTest.transform.position)
                {
                    foundPlayer = true;
                    canSeePlayer = true;
                    Vector2 direction = pcTest.transform.position - transform.position;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
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
            else
            {
                foundPlayer = false;
            }


            if (isAttacking)
            {
                pcTest.TakeDamage(enemyStats.enemyDamage);
            }
        }
    }
    private void FixedUpdate()
    {
        if (isPaused) {
            timeintoslow += Time.deltaTime;
            return;
        }
        if (isReplayingActions)
        {
            ObjectState currentState = recordedStates[stateIndex];
            rb.linearVelocity = currentState.currentVelocity;
            transform.rotation = currentState.currentRotation;
            if (!currentlyFiring)
            {
                timeToFire += Time.fixedDeltaTime;
            }
            if (currentState.currentlyFiring)
            {
                timeToFire = 0;
                StartCoroutine(BeginFiringSequence());
            }
            stateIndex++;
            if (stateIndex == recordedStates.Count)
            {
                isReplayingActions = false;
            }
        }
        else
        {
            bool firedThisTick = false;
            if (gameManager.isPlayerAlive)
            {
                if (foundPlayer)
                {
                    float factor = 1.0f;
                    if (canSeePlayer)
                    {
                        factor = (transform.position - pcTest.transform.position).magnitude > comfortableDistance ? 1.0f : -1.0f * movementPattern.BackoffSpeedFactor;
                    }
                    rb.linearVelocity = factor * transform.right * enemySpeed;
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
                                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 5.0f;
                                randomTarget = transform.position + new Vector3(offset.x, offset.y, 0);
                            }
                            Vector2 direction = randomTarget - transform.position;
                            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
                            rb.linearVelocity = transform.right * enemySpeed;
                            break;
                    }
                }
            }

            if (canSeePlayer)
            {
                if (!currentlyFiring)
                {
                    timeToFire += Time.fixedDeltaTime;
                }
                if (timeToFire >= weapon.fireRate)
                {
                    firedThisTick = true;
                    timeToFire = 0;
                    StartCoroutine(BeginFiringSequence());
                }
            }
            recordedStates.Add(new ObjectState(rb.linearVelocity, transform.position, transform.rotation, firedThisTick));
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            if(movementPattern.idleBehavior == EnemyMovementPattern.idleBehaviors.RandomWalk && !foundPlayer){
                Vector2 offset = UnityEngine.Random.insideUnitCircle.normalized * 5.0f;
                randomTarget = transform.position + new Vector3(offset.x, offset.y, 0);
                RaycastHit2D hit = Physics2D.Linecast(transform.position, randomTarget, terrainMask);
                int tries = 0;
                while(hit.collider != null){
                    offset = UnityEngine.Random.insideUnitCircle.normalized * 5.0f;
                    randomTarget = transform.position + new Vector3(offset.x, offset.y, 0);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = false;
        }
    }

    IEnumerator LeashPlayer()
    {
        if(movementPattern.LeashTime > 0)
        {
            float elapsedTime = 0;
            while(true)
            {
                yield return new WaitForSeconds(checkInterval);
                RaycastHit2D hit = Physics2D.Linecast(transform.position, pcTest.transform.position, terrainMask);
                if(hit.collider != null && hit.collider.transform.position == pcTest.transform.position){
                    foundPlayer = true;
                    canSeePlayer = true;
                    break;
                } else {
                    elapsedTime += checkInterval;
                    if(elapsedTime >= movementPattern.LeashTime){
                        foundPlayer = false;
                        canSeePlayer = false;
                        break;
                    }
                }
            }
        }
    }

    IEnumerator BeginFiringSequence()
    {
        currentlyFiring = true;
        for (int i = 0; i < weapon.bulletPattern.fireCount; i++)
        {
            FireOnce(i);
            if (i < weapon.bulletPattern.fireCount - 1)
            {
                yield return new WaitForSeconds(weapon.bulletPattern.timeBetweenFiring * slowFactor);
            }
        }
        currentlyFiring = false;

    }

    private void FireOnce(int volleyIndex)
    {
        float baseAngle = transform.eulerAngles.z + weapon.bulletPattern.rotateBetweenFiring * volleyIndex;
        if (weapon.bulletPattern.bulletCount == 1)
        {
            if (weapon.bulletPattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Even)
            {
                CreateBullet(baseAngle);
            }
            else
            {
                CreateBullet(baseAngle + UnityEngine.Random.Range(-weapon.bulletPattern.firingAngle / 2, weapon.bulletPattern.firingAngle / 2));
            }
        }
        else
        {
            List<float> angleChanges = new List<float>();
            float angleChange = -weapon.bulletPattern.firingAngle / 2;
            float changeStep = weapon.bulletPattern.firingAngle / (weapon.bulletPattern.bulletCount - 1);
            switch (weapon.bulletPattern.bulletDistribution)
            {
                case BulletPattern.bulletDistributionTypes.Even:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChanges.Add(angleChange);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.SemiRandom:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        float randomAdjustment = UnityEngine.Random.Range(-changeStep / 3, changeStep / 3);
                        angleChanges.Add(angleChange + randomAdjustment);
                        angleChange += changeStep;
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Random:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChange = UnityEngine.Random.Range(-weapon.bulletPattern.firingAngle / 2, weapon.bulletPattern.firingAngle / 2);
                        angleChanges.Add(angleChange);
                    }
                    break;
                case BulletPattern.bulletDistributionTypes.Radial:
                    for (int i = 0; i < weapon.bulletPattern.bulletCount; i++)
                    {
                        angleChange = 0 + (weapon.bulletPattern.firingAngle/weapon.bulletPattern.bulletCount * i);
                        angleChanges.Add(angleChange);
                    }
                    break;
            }
            foreach (float change in angleChanges)
            {
                if (weapon.bulletPattern.bulletDistribution == BulletPattern.bulletDistributionTypes.Radial)
                {
                    CreateBullet(change + (volleyIndex * weapon.bulletPattern.rotateBetweenFiring));
                }
                else
                {
                    CreateBullet(baseAngle + change + (volleyIndex * weapon.bulletPattern.rotateBetweenFiring));   
                }
            }   
        }
    }

    private void CreateBullet(float angle)
    {
        Vector2 spawnVector = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        float offsetDistance = GetComponent<Collider2D>().bounds.extents.magnitude * 0.9f;
        Vector3 spawnPosition = transform.position + (Vector3)(spawnVector * offsetDistance);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        GameObject spawnedBullet = Instantiate(weapon.bulletType, spawnPosition, rotation);
        Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
        
        bulletAttributes.InitBullet(weapon.bulletSpeed, weapon.bulletLifeTime, weapon.bulletDamage, "enemy",enemyStats.enemyColor);
        if(isPaused) bulletAttributes.PauseBullet(5.0f - timeintoslow, slowFactor);
    }

    public void isAlive(bool status)
    {
        gameObject.SetActive(status);
        BoundHealthbar.SetActive(status);
    }

    public void Erase()
    {
        Destroy(BoundHealthbar);
        Destroy(gameObject);
    }

    public void Reset()
    {
        Debug.Log($"Enemy state count: {recordedStates.Count}");
        stateIndex = 0;
        StopAllCoroutines();
        RefreshStats();
        enemyStats.Reset();
        timeToFire = weapon.fireRate - 0.6f;
        randomTarget = transform.position;
        rb.linearVelocityX = 0;
        rb.linearVelocityY = 0;
        isReplayingActions = true;
        isAlive(false);
    }

    private void UpdateEnemyColor()
    {

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = enemyStats.enemyColor.ToRGB();
        }
        

        //Debug.Log($"Enemy Color - H:{enemyStats.enemyColor.H:F1}, S:{enemyStats.enemyColor.S:F1}, L:{enemyStats.enemyColor.L:F1}");
    }
//********************************Enemy Pause********************************
    public void PauseEnemy(float pauseDuration, float pauseStrength)
    {
        if (!isPaused)
        {
            StartCoroutine(PauseCoroutine(pauseDuration, pauseStrength));
        }
    }
    
    private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength)
    {
        savedVelocity = rb.linearVelocity;
        rb.linearVelocity /= pauseStrength;
        slowFactor = pauseStrength;
        isPaused = true;
        
        yield return new WaitForSeconds(pauseDuration);
        
        ResumeEnemy();
    }
    
    public void ResumeEnemy()
    {
        if (isPaused)
        {
            rb.linearVelocity = savedVelocity;
            isPaused = false;
            slowFactor = 1.0f;
            timeintoslow = 0.0f;
        }
    }    

    
}
