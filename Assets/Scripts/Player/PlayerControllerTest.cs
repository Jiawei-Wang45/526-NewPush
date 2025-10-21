using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System;
public class PlayerControllerTest : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;
    public PlayerInput playerInput;
    public GhostController eraserGhost;
    public ShieldGhost shieldGhost;
    public GameObject StandShape;
    public GameObject HitboxShape;
    public GameObject ReturnPosition;
    public GameObject AbilityHint;
    private GameObject returnPositionInstance;
    private float speed;
    private Vector2 movement;
    public Vector2 initialPosition;
    public int abilityEnum = 0;
    [Header("Analytics")]
    [SerializeField] private SendToGoogle sendToGoogle;
    public Bullet_Eraser eraserAbility;
    private bool eraserUsed = true;

    private bool isFiring = false;
    private bool hasFired = false;

    private bool recordFireAction = false;
    private bool recordEquipAction = false;
    private string recordAbility = "";
    private float pausedTimeRemaining = 0;

    private List<ObjectState> recordedStates = new List<ObjectState>();

    public GameManager gameManager;

    private float fireTimer;
    public Transform firePoint;
    public PlayerWeapon currentWeapon;

    // Ghost dash settings
    [Header("Ghost Dash")]
    public float dashMultiplier = 2.0f; // how many times faster during dash
    public float dashDuration = 3.0f; // seconds the dash lasts
    public float dashCooldown = 3.0f; // seconds before dash can be used again
    private bool isDashing = false;

    [Header("Ghost Recording")]
    public float ghostRecordingDuration = 5.0f;

    public float ghostRecordingCooldown = 10.0f;

    public GameObject dashEffectPrefab; // optional visual effect instantiated during dash
    private bool abilityOnCooldown = false;

    private Vector2 savedVelocity;
    private bool savedIsFiring;
    private bool isPaused = false;
    private bool isRecording = false;
    private Vector2 savedPosition;
    private Quaternion savedRotation;
    public void RefreshStats()
    {
        speed = stats.movementSpeed;
    }

    private void Awake()
    {
        playerInput = new PlayerInput();
        // ensure sendToGoogle is assigned
        if (sendToGoogle == null)
        {
            // try to find any enabled instance first
            sendToGoogle = FindFirstObjectByType<SendToGoogle>();
            // if still null, try to find inactive instances (Unity API that returns array)
            if (sendToGoogle == null)
            {
                try
                {
                    SendToGoogle[] all = FindObjectsByType<SendToGoogle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (all != null && all.Length > 0)
                    {
                        sendToGoogle = all[0];
                    }
                }
                catch
                {
                    // fallback: use FindObjectOfType that supports inactive when available
                    try
                    {
                        sendToGoogle = FindObjectOfType<SendToGoogle>(true);
                    }
                    catch { }
                }
            }
        }

        Debug.Log($"[PlayerControllerTest] Awake auto-assign sendToGoogle: {sendToGoogle != null} (object: {sendToGoogle?.gameObject.name})");
    }
    private void OnEnable()
    {
        playerInput.Default.Enable();
    }
    private void OnDisable()
    {
        playerInput.Default.Disable();
    }
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        initialPosition = transform.position;
        RefreshStats();

        //player movement binding
        playerInput.Default.Move.performed += OnMoveTriggered;
        playerInput.Default.Move.canceled += OnMoveTriggered;

        //fire input binding
        playerInput.Default.Fire.performed += OnFireTriggered;
        playerInput.Default.Fire.canceled += OnFireTriggered;

        playerInput.Default.Reload.performed += OnReloadTriggered;

        playerInput.Default.PauseBullets.performed += OnAbilityTriggered;
        playerInput.Default.FireEraser.performed += OnFireEraser;
        if (currentWeapon)
        {
            fireTimer = currentWeapon.weaponFireRate;
            stats.currentWeapon = currentWeapon;
            stats.currentAmmo = currentWeapon.weaponAmmo;
        }

    }


    private void Update()
    {
        //float inputX = Input.GetAxisRaw("Horizontal");
        //float inputY = Input.GetAxisRaw("Vertical");
        //movement=new Vector2(inputX, inputY);

        // branch between active and passive weapons
        //if (isPaused) return;
        UpdatePlayerColor();

        if (currentWeapon == null)
            return;
        if (currentWeapon.weaponType == WeaponType.Passive)
        {
            Fire();
            recordFireAction = true;
        }
        else
        {
            if (currentWeapon.triggerType == TriggerType.Automatic)
            {
                //if (Input.GetMouseButtonDown(0))
                //{
                //    isFiring = true;
                //}
                //if (Input.GetMouseButtonUp(0))
                //{
                //    isFiring = false;
                //}
                if (isFiring && fireTimer >= 1 / currentWeapon.weaponFireRate)
                {
                    Fire();
                    recordFireAction = true;
                    fireTimer = 0;
                }
            }
            else if (currentWeapon.triggerType == TriggerType.SemiAutomatic)
            {
                //if (Input.GetMouseButtonDown(0))
                //{
                //    isFiring = true;
                //}
                //if (Input.GetMouseButtonUp(0))
                //{
                //    isFiring = false;
                //    hasFired = false;
                //} 
                if (isFiring && !hasFired && fireTimer >=  1 / currentWeapon.weaponFireRate)
                {
                    Fire();
                    recordFireAction = true;
                    fireTimer = 0;
                    hasFired = true;
                }
                else
                {
                    fireTimer += Time.fixedDeltaTime;
                }
            }
        }
        Debug.DrawLine(firePoint.position, firePoint.position + firePoint.transform.right, Color.red, 0);
    }

    private void FixedUpdate()
    {
        //if (isPaused) return;
        AbilityHint.SetActive(!abilityOnCooldown);
        rb.linearVelocity = movement * speed;
        if (isRecording)
        {
            if (recordEquipAction)
            {
                recordedStates.Add(new ObjectState(rb.linearVelocity, rb.position, firePoint.rotation, recordFireAction, currentWeapon, recordAbility));
                recordEquipAction = false;

            }
            else
            {
                recordedStates.Add(new ObjectState(rb.linearVelocity, rb.position, firePoint.rotation, recordFireAction, null, recordAbility));
            }
            if (recordFireAction)
            {
                recordFireAction = false;
            }
            recordAbility = "";
        }
        if (pausedTimeRemaining > 0)
        {
            pausedTimeRemaining = Math.Max(0, pausedTimeRemaining - Time.deltaTime);
        }
        if (currentWeapon.triggerType == TriggerType.Automatic)
        {
            fireTimer = fireTimer < currentWeapon.weaponFireRate ? fireTimer + Time.fixedDeltaTime : fireTimer;
        }
    }
    private void Fire(string fireType = "bullet")
    {
        if(fireType == "eraser")
        {
            Bullet_Eraser eraser = Instantiate(eraserAbility, firePoint.position, firePoint.rotation);
            return;
        }

        if (!stats.CanFire()) return;

        stats.ConsumeAmmo(1);

        float bulletTiltAngle = -(currentWeapon.weaponBulletAmount - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0; i < currentWeapon.weaponBulletAmount; i++)
        {
            GameObject spawnedBullet = Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation);
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();

            //float timeScaleFactor = isPaused ? 0.5f : 1.0f;
            spawnedBullet.transform.Rotate(0, 0, bulletTiltAngle + UnityEngine.Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread));
            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed, pausedTimeRemaining > 0 ? pausedTimeRemaining : currentWeapon.weaponBulletLifeTime, currentWeapon.weaponBulletDamage, "player", stats.playerColor);
            bulletTiltAngle += currentWeapon.weaponFiringAngle;
        }

    }
    public void EquipWeapon(PlayerWeapon weapon)
    {
        Debug.Log("Player equipping weapon");
        currentWeapon = weapon;
        recordEquipAction = true;
    }
    public void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);
    }
    private void OnMoveTriggered(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void OnFireTriggered(InputAction.CallbackContext context)
    {
        if (currentWeapon == null) return;
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                isFiring = true;
                break;
            case InputActionPhase.Canceled:
                isFiring = false;
                if (currentWeapon.triggerType == TriggerType.SemiAutomatic)
                    hasFired = false;
                break;
        }
    }

    private void OnFireEraser(InputAction.CallbackContext context)
    {
        if (!eraserUsed)
        {
            recordAbility = "eraser";
            eraserUsed = true;
            gameManager.DisplayEraserMessage(false);
            Fire("eraser");
        }
    }

    private void OnReloadTriggered(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            stats.StartReload();
        }
    }



    public List<ObjectState> sendStates()
    {
        if (currentWeapon)
        {
            ObjectState stateToChange = recordedStates[0];
            stateToChange.usingNewWeapon = currentWeapon;
            GetComponent<PlayerStats>().currentWeapon = currentWeapon;
            recordedStates[0] = stateToChange;
        }
        Debug.Log($"Sending state list of size {recordedStates.Count}");
        return recordedStates;
    }

    private void UpdatePlayerColor()
    {

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color baseColor = stats.playerColor.ToRGB();
            if (stats != null && stats.isInvincible)
            {
                // half transparent when invincible
                baseColor.a = 0.5f;
            }
            else
            {
                baseColor.a = 1f;
            }
            spriteRenderer.color = baseColor;
        }


        //Debug.Log($"Player Color - H:{stats.playerColor.H:F1}, S:{stats.playerColor.S:F1}, L:{stats.playerColor.L:F1}");
    }

    public void Reset()
    {
        stats.Reset();
        stats.ResetH();
        transform.position = initialPosition;
        rb.linearVelocityX = 0;
        rb.linearVelocityY = 0;
        isFiring = false;
        hasFired = false;
        recordFireAction = false;
        fireTimer = 0;
        recordedStates.Clear();
    }

    public void UponWaveClear()
    {
        stats.health = stats.maxHealth;
        recordedStates.Clear();
        initialPosition = transform.position;
        stats.ResetH();
    }

    private void OnAbilityTriggered(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            switch (abilityEnum)
            {
                case 0:
                    PauseAllPausable(2.0f, 8.0f);
                    break;
                case 1:
                    PauseAllPausable(2.0f, 0.1f);
                    break;

                case 3:
                    ActiveRecordGhost("shield");
                    break;
                case 4:
                    GhostDash();
                    break;
                case 5:
                    ActiveRecordGhost("bulletClearer");
                    break;
                default:
                    break;
            }

            // Send analytics when ability is used
            GameManager gm = FindFirstObjectByType<GameManager>();
            int waveToSend = gm != null ? gm.CurrentWave : 0;
            Debug.Log($"[PlayerControllerTest] sendToGoogle assigned? {sendToGoogle != null} wave={waveToSend} pos={transform.position}");
            if (sendToGoogle != null)
            {
                sendToGoogle.SendAbilityUse(transform.position, waveToSend);
            }
        }
    }

    private void ActiveRecordGhost(string ghostType)
    {
        if (!abilityOnCooldown)
        {
            abilityOnCooldown = true;
            if (isRecording) return;
            GetComponent<SpriteRenderer>().sortingOrder -= 3;
            returnPositionInstance = Instantiate(ReturnPosition, transform.position, transform.rotation);
            if (ghostType == "shield")
            {
                StandShape.SetActive(true);
                StandShape.GetComponent<SpriteRenderer>().sortingOrder += 3;
                HitboxShape.SetActive(true);
                HitboxShape.GetComponent<SpriteRenderer>().sortingOrder += 3;
            } else
            {
                gameManager.DisplayEraserMessage(true);
                eraserUsed = false;
            }
            isRecording = true;
            recordedStates.Clear();
            recordedStates.Add(new ObjectState(rb.linearVelocity, savedPosition, savedRotation, currentWeapon));
            PauseAllPausable(ghostRecordingDuration, 20.0f);
            pausedTimeRemaining = ghostRecordingDuration;
            StartCoroutine(RecordingCoroutine(ghostType));
        }
    }

    private IEnumerator RecordingCoroutine(string ghostType)
    {
        savedPosition = transform.position;
        savedRotation = transform.rotation;
        yield return new WaitForSeconds(ghostRecordingDuration);
        eraserUsed = true;
        gameManager.DisplayEraserMessage(false);
        List<ObjectState> playerStates = sendStates();
        Destroy(returnPositionInstance.gameObject);
        GetComponent<SpriteRenderer>().sortingOrder += 3;
        StandShape.SetActive(false);
        StandShape.GetComponent<SpriteRenderer>().sortingOrder -= 3;
        HitboxShape.SetActive(false);
        HitboxShape.GetComponent<SpriteRenderer>().sortingOrder -= 3;
        if (ghostType == "shield")
        {
            Debug.Log("Making shield ghost");
            ShieldGhost newGhost = Instantiate(shieldGhost);
            newGhost.InitializeGhost(savedPosition, playerStates);
        } else
        {
            Debug.Log("Making eraser ghost");
            GhostController newGhost = Instantiate(eraserGhost);
            newGhost.InitializeGhost(savedPosition, playerStates);
        }
        transform.position = savedPosition;
        transform.rotation = savedRotation;
        isRecording = false;

        yield return new WaitForSeconds(ghostRecordingDuration + ghostRecordingCooldown);
        abilityOnCooldown = false;
    }

    //********************************Pause All Pausable********************************
    public void PauseAllPausable(float pauseDuration, float pauseStrength)
    {
        GameObject[] pausableObjects = GameObject.FindGameObjectsWithTag("Pausable");
        foreach (GameObject obj in pausableObjects)
        {
            Bullet_Default bullet = obj.GetComponent<Bullet_Default>();
            if (bullet != null)
            {
                bullet.PauseBullet(pauseDuration, pauseStrength);
            }

            EnemyController enemy = obj.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.PauseEnemy(pauseDuration, pauseStrength);
            }

            PlayerControllerTest player = obj.GetComponent<PlayerControllerTest>();
            if (player != null)
            {
                player.PausePlayer(pauseDuration);
            }
        }
    }
    //********************************Player Pause********************************
    public void PausePlayer(float pauseDuration)
    {
        if (!isPaused)
        {
            StartCoroutine(PauseCoroutine(pauseDuration));
        }
    }

    private IEnumerator PauseCoroutine(float pauseDuration)
    {
        //savedVelocity = rb.linearVelocity;
        //savedIsFiring = isFiring;
        //rb.linearVelocity = Vector2.zero;
        //isFiring = false;
        isPaused = true;

        yield return new WaitForSeconds(pauseDuration);

        ResumePlayer();
    }

    public void ResumePlayer()
    {
        if (isPaused)
        {
            //rb.linearVelocity = savedVelocity;
            //isFiring = savedIsFiring;
            isPaused = false;
        }
    }

//********************************Ghost Dash********************************
    public void GhostDash()
    {
        // Prevent starting another dash while on cooldown or already dashing
        if (abilityOnCooldown || isDashing) return;
        StartCoroutine(GhostDashCoroutine());
    }

    private IEnumerator GhostDashCoroutine()
    {
        abilityOnCooldown = true;
        isDashing = true;

        // save current speed so we can restore it later
        float savedSpeed = speed;

        // apply dash speed
        speed = stats != null ? stats.movementSpeed * dashMultiplier : speed * dashMultiplier;

        // make player invincible during dash
        stats.isInvincible = true;
        PauseAllPausable(dashDuration, 5.0f);

        yield return new WaitForSeconds(dashDuration);

        // end dash
        isDashing = false;
        stats.isInvincible = false;
        // restore speed (use current stats.movementSpeed in case it changed while dashing)
        speed = stats != null ? stats.movementSpeed : savedSpeed;

        // start cooldown wait
        yield return new WaitForSeconds(dashCooldown);
        abilityOnCooldown = false;
    }
}
