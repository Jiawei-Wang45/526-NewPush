using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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

    }


    private void Update()
    {
        //float inputX = Input.GetAxisRaw("Horizontal");
        //float inputY = Input.GetAxisRaw("Vertical");
        //movement=new Vector2(inputX, inputY);

        // branch between active and passive weapons

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
            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed, currentWeapon.weaponBulletLifeTime, currentWeapon.weaponBulletDamage, "player",stats.playerColor);
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

}
