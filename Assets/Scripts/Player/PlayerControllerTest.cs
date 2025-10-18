using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class PlayerControllerTest : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats stats;
    public PlayerInput playerInput;
    private float speed;
    private Vector2 movement;
    public Vector2 initialPosition;

    private bool isFiring=false;
    private bool hasFired = false;

    private bool recordFireAction = false;
    private bool recordEquipAction = false;

    private List<ObjectState> recordedStates = new List<ObjectState>();

    private float fireTimer;
    public Transform firePoint;
    public PlayerWeapon currentWeapon;

    private Vector2 savedVelocity;
    private bool savedIsFiring;
    private bool isPaused = false;

    public void RefreshStats()
    {
        speed = stats.movementSpeed;
    }

    private void Awake()
    {
        playerInput = new PlayerInput();
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
        rb= GetComponent<Rigidbody2D>();
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

        playerInput.Default.PauseBullets.performed += OnPauseAllTriggered;

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
                if (isFiring && fireTimer >= 1 / currentWeapon.weaponBulletSpeed)
                {
                    Fire();
                    recordFireAction = true;
                    fireTimer = 0;
                }
                else
                {
                    fireTimer += Time.fixedDeltaTime;
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
                if (isFiring && !hasFired && fireTimer >= 1 / currentWeapon.weaponBulletSpeed)
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
        rb.linearVelocity = movement * speed;
        if (recordEquipAction)
        {
            recordedStates.Add(new ObjectState(rb.linearVelocity, firePoint.rotation, recordFireAction, currentWeapon));
            recordEquipAction = false;

        } else {
            recordedStates.Add(new ObjectState(rb.linearVelocity, firePoint.rotation, recordFireAction));
        }
        if (recordFireAction)
        {
            recordFireAction = false;
        }
    }

    private void Fire()
    {

        if (!stats.CanFire()) return;

        stats.ConsumeAmmo(1f);

        float bulletTiltAngle = -(currentWeapon.weaponBulletAmount - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0;i<currentWeapon.weaponBulletAmount;i++)
        {
            GameObject spawnedBullet=Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation);
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();

            float timeScaleFactor = isPaused ? 0.5f : 1.0f;

            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed * timeScaleFactor, currentWeapon.weaponBulletLifeTime, currentWeapon.weaponBulletDamage, "player",stats.playerColor);
            spawnedBullet.transform.Rotate(0, 0, bulletTiltAngle+Random.Range(-currentWeapon.weaponBulletSpread,currentWeapon.weaponBulletSpread));
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

    private void OnReloadTriggered(InputAction.CallbackContext context){

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
            spriteRenderer.color = stats.playerColor.ToRGB();
        }
        

        Debug.Log($"Player Color - H:{stats.playerColor.H:F1}, S:{stats.playerColor.S:F1}, L:{stats.playerColor.L:F1}");
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
    }

    private void OnPauseAllTriggered(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PauseAllPausable(2.0f); // 暂停2秒
        }
    }

    public void PauseAllPausable(float pauseDuration)
    {
       GameObject[] pausableObjects = GameObject.FindGameObjectsWithTag("Pausable");
       foreach (GameObject obj in pausableObjects)
       {
           // 暂停子弹
           Bullet_Default bullet = obj.GetComponent<Bullet_Default>();
           if (bullet != null)
           {
               bullet.PauseBullet(pauseDuration);
           }
           
           // 暂停敌人
           EnemyController enemy = obj.GetComponent<EnemyController>();
           if (enemy != null)
           {
               enemy.PauseEnemy(pauseDuration);
           }
           
           // 暂停玩家
           PlayerControllerTest player = obj.GetComponent<PlayerControllerTest>();
           if (player != null)
           {
               player.PausePlayer(pauseDuration);
           }
       }
    }
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
}
