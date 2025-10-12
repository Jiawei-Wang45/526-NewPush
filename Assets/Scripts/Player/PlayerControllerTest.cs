using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerControllerTest : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private PlayerHSLSystem hslSystem;
    public PlayerInput playerInput;
    private float speed;
    private Vector2 movement;

    private bool isFiring=false;
    private bool hasFired = false;
    private float fireTimer;
    public Transform firePoint;
    public PlayerWeapon currentWeapon;
    public void RefreshStats()
    {
        speed = playerStats.movementSpeed;
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
        playerStats= GetComponent<PlayerStats>();
        hslSystem = GetComponent<PlayerHSLSystem>();
        RefreshStats();

        //player movement binding
        playerInput.Default.Move.performed += OnMoveTriggered;
        playerInput.Default.Move.canceled += OnMoveTriggered;

        //fire input binding
        playerInput.Default.Fire.performed += OnFireTriggered;
        playerInput.Default.Fire.canceled += OnFireTriggered;

    }


    private void Update()
    {
        //float inputX = Input.GetAxisRaw("Horizontal");
        //float inputY = Input.GetAxisRaw("Vertical");
        //movement=new Vector2(inputX, inputY);

        // branch between active and passive weapons
        if (currentWeapon == null)
            return;
        if (currentWeapon.weaponType== WeaponType.Passive)
        {
            Fire();
        }
        else
        {
            if (currentWeapon.triggerType== TriggerType.Automatic)
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
                    fireTimer = 0;       
                }
                else
                {
                    fireTimer += Time.deltaTime;
                }
            }
            else if (currentWeapon.triggerType==TriggerType.SemiAutomatic)
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
                    fireTimer = 0;
                    hasFired = true;
                }
                else
                {
                    fireTimer += Time.deltaTime;
                }
            }
        }
        Debug.DrawLine(firePoint.position, firePoint.position + firePoint.transform.right, Color.red, 0);
    }
    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    public void Fire()
    {
        // 检查是否可以开火（色相偏移检查）
        if (hslSystem != null && !hslSystem.CanFire())
        {
            Debug.Log("Cannot fire! Hue shift too high: " + hslSystem.GetHueShiftPercentage() * 100f + "%");
            return; // 色相偏移过大，无法开火
        }
        
        // 检查弹药是否足够
        if (hslSystem != null && !currentWeapon.hasInfiniteAmmo)
        {
            int requiredAmmo = currentWeapon.weaponBulletAmount * currentWeapon.ammoPerShot;
            if (hslSystem.currentAmmo < requiredAmmo)
            {
                Debug.Log("Not enough ammo! Current: " + hslSystem.currentAmmo + ", Required: " + requiredAmmo);
                return; // 弹药不足，无法开火
            }
        }
        
        float bulletTiltAngle = -(currentWeapon.weaponBulletAmount - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0;i<currentWeapon.weaponBulletAmount;i++)
        {
            GameObject spawnedBullet=Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation);
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed, currentWeapon.weaponBulletLifeTime, currentWeapon.weaponBulletDamage, "player");
            spawnedBullet.transform.Rotate(0, 0, bulletTiltAngle+Random.Range(-currentWeapon.weaponBulletSpread,currentWeapon.weaponBulletSpread));
            bulletTiltAngle += currentWeapon.weaponFiringAngle;
        }
        
        // 消耗弹药
        if (hslSystem != null && !currentWeapon.hasInfiniteAmmo)
        {
            int ammoToConsume = currentWeapon.weaponBulletAmount * currentWeapon.ammoPerShot;
            hslSystem.ConsumeAmmo(ammoToConsume);
        }
    }
    private void EquipWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;
        
        // 如果HSL系统存在，设置新武器的弹药量
        if (hslSystem != null && weapon != null)
        {
            hslSystem.SetMaxAmmo(weapon.maxAmmo);
        }
    }
    public void TakeDamage(float damage)
    {
        playerStats.TakeDamage(damage);
    }
    
    public void ReloadAmmo(int amount)
    {
        if (hslSystem != null)
        {
            hslSystem.ReloadAmmo(amount);
        }
    }
    
    public void FullReload()
    {
        if (hslSystem != null && currentWeapon != null)
        {
            int ammoNeeded = currentWeapon.maxAmmo - hslSystem.currentAmmo;
            hslSystem.ReloadAmmo(ammoNeeded);
        }
    }
    private void OnMoveTriggered(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void OnFireTriggered(InputAction.CallbackContext context)
    {
        if (currentWeapon == null) return;
        switch(context.phase)
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

}
