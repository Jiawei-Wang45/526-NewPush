using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireAbility: MonoBehaviour
{
    private PlayerStats stats;
    private PlayerControllerTest pc;
    private PlayerInput playerInput;

    // firing variables
    [Header("Basic Firing parameters")]
    private float fireTimer;
    public Transform firePoint;
    public PlayerWeapon currentWeapon;
    private bool isFiring = false;
    private bool isCoroutineRunning = false;
    public GameObject laser;
    private GameObject sword;
    private Sword swordAttributes;
    //reload variables
    [Header("Reloading parameters")]
    public float reloadTime = 1.5f;
    private int maxAmmo;
    private int currentAmmo;
    private bool isReloading = false;
    //reload UI
    [Header("Reload UI settings")]
    public GameObject reloadBar;
    public GameObject handle;
    public float targetOffsetX;

    //delegate declaration
    //public delegate void FireDelegate();
    //public event FireDelegate OnFire;

    #region initialization
    private void Awake()
    {
        pc= GetComponent<PlayerControllerTest>();
        stats=GetComponent<PlayerStats>();
    }
    private void Start()
    {
        //maxAmmo = currentWeapon.maxAmmoNums;
        //currentAmmo = maxAmmo;
        reloadBar.SetActive(false);

        playerInput = pc.playerInput;
        //fire input binding
        playerInput.Default.Fire.started += OnFireTriggered;
        playerInput.Default.Fire.canceled += OnFireTriggered;

        //reload input binding
        playerInput.Default.Reload.performed += OnReloadTriggered;

        //reset binding
        GameManager.instance.onReset += ResetStates;
    }
    #endregion

    #region Update
    private void Update()
    {
        if(currentWeapon != null && currentWeapon.weaponClass == WeaponClass.Melee && sword != null && !swordAttributes.Swinging())
        {
            sword.transform.rotation = firePoint.rotation;
        }
    }
    #endregion

    #region Fire
    private void Fire()
    {
        if(isReloading){
            return;
        }
        //OnFire?.Invoke();
        ConsumeAmmo(1);

        // Increment weapon use count for analytics
        if (GameManager.instance != null)
        {
            GameManager.instance.IncrementWeaponUseCount();
        }

        float bulletTiltAngle = -(currentWeapon.weaponBulletInOneShot - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0; i < currentWeapon.weaponBulletInOneShot; i++)
        {
            float bulletSpeed = currentWeapon.weaponBulletSpeed;
            float bulletDamage = currentWeapon.weaponBulletDamage;
            int bounceCount = 0;
            if(PauseManager.instance.isPausing)
            {
                if(pc.combinationIndex == 1)
                {
                    Quaternion laserRotation = firePoint.rotation * Quaternion.Euler(0, 0, 90.0f+ bulletTiltAngle + Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread));
                    
                    GameObject spawnedLaser = Instantiate(laser, firePoint.position - laserRotation * Vector2.up * 50.0f, laserRotation);
                    Bullet_Laser laserAttributes = spawnedLaser.GetComponent<Bullet_Laser>();
                    laserAttributes.InitBullet(0.0f, bulletDamage*5.0f,stats.playerColor);
                    continue;
                }
            }
            if(stats.isInvincible)
            {
                Debug.Log("Invincible: " + stats.isInvincible);
                if(pc.combinationIndex == 5)
                {
                    bulletDamage *= 2.0f;
                    bounceCount = 7;
                }
            }
            GameObject spawnedBullet = Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, bulletTiltAngle + Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread)));
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();     
            bulletAttributes.InitBullet(bulletSpeed, bulletDamage,stats.playerColor, bounceCount);
            if(PauseManager.instance.isPausing)
            {
                if(pc.combinationIndex == 2)
                {
                    bulletAttributes.PauseStart(PauseManager.instance.activePauseStrength * 2);
                }
            }
            bulletTiltAngle += currentWeapon.weaponFiringAngle;
        }
        if(pc.combinationIndex == 6)
        {
            // Apply knockback in the opposite direction of firing
            Vector2 knockbackDirection = -(firePoint.rotation * Vector2.right);
            pc.AddForcePlayer(knockbackDirection * 20.0f);
        }
        if(pc.combinationIndex == 2)
        {
            PauseManager.instance.extendDuration += 1.0f;
        }
        if (currentAmmo <= 0)
        {
            ActivateReload();
        }
    }

    private void Swing(float swingDuration)
    {
        //OnFire?.Invoke();

        // Increment weapon use count for analytics
        if (GameManager.instance != null)
        {
            GameManager.instance.IncrementWeaponUseCount();
        }
        swordAttributes.Swing(swingDuration);
    }

    public void ConsumeAmmo(int amount)
    {
        currentAmmo -= amount;
        // update player color saturation based on ammo proportion
        if (stats != null && maxAmmo > 0)
        {
            stats.playerColor.S = (currentAmmo / (float)maxAmmo) * 100f;
        }
    }
    private void OnFireTriggered(InputAction.CallbackContext context)
    {
        if (currentWeapon == null) return;
        switch (context.phase)
        {
            case InputActionPhase.Started:
                isFiring = true;
                StartCoroutine(FireCoroutine());
                break;
            case InputActionPhase.Canceled:
                isFiring = false;
                break;
        }
    }
    #endregion
    #region Reload
    private void OnReloadTriggered(InputAction.CallbackContext context)
    {
        ActivateReload();
    }
    public void ActivateReload()
    {
        if (isReloading) return;
        isReloading = true;
        StartCoroutine(ReloadCoroutine());
    
    }

    private IEnumerator FireCoroutine()
    {
        if (currentWeapon == null || isCoroutineRunning)
            yield break;
        switch (currentWeapon.weaponClass)
        {
            case WeaponClass.Shotgun:
            case WeaponClass.SemiAuto:
                Fire();
                isCoroutineRunning = true;
                yield return new WaitForSeconds(1f);
                isCoroutineRunning = false;
                break;
            case WeaponClass.FullAuto:
                isCoroutineRunning = true;
                Fire();
                float fireIntervalMultiplier = (pc.combinationIndex == 0 && PauseManager.instance.isPausing) ? 0.3f : 1.0f;
                yield return new WaitForSeconds(1 / currentWeapon.weaponFireRate * fireIntervalMultiplier);
                isCoroutineRunning = false;
                if (isFiring)
                {
                    StartCoroutine(FireCoroutine());
                }
                break;
            case WeaponClass.Melee:
                isCoroutineRunning = true;
                if(pc.combinationIndex == 7)
                {
                    // Dash forward with invincible effect
                    Vector2 knockbackDirection = (firePoint.rotation * Vector2.right);
                    pc.AddForcePlayer(knockbackDirection * 10.0f);
                }
                Swing((1 / currentWeapon.weaponFireRate )/ 2.0f);
                yield return new WaitForSeconds(1 / currentWeapon.weaponFireRate);
                isCoroutineRunning = false;
                if (isFiring)
                {
                    StartCoroutine(FireCoroutine());
                }
                break;
            case WeaponClass.None:
                break;
            default:
                break;
        }
    }

    public void SetAmmoToMax()
    {
        currentAmmo = maxAmmo;
        if (stats != null)
        {
            stats.playerColor.S = 100f;
        }
        ResetReload();
    }

    private IEnumerator ReloadCoroutine()
    {
        reloadBar.SetActive(true);
        float accumulateTime = 0;
        while (true)
        {
            setHandleOffsetX(accumulateTime / reloadTime);
            accumulateTime += Time.deltaTime;
            if (accumulateTime > reloadTime)
                break;
            yield return null;
        }
        SetAmmoToMax();
    }
    private void setHandleOffsetX(float percent)
    {
        Vector3 handleLocalPos = handle.transform.localPosition;
        handleLocalPos.x = (percent - 0.5f) * targetOffsetX;
        handle.transform.localPosition = handleLocalPos;
    }
    public void ResetReload()
    {
        reloadBar.SetActive(false);
        isReloading = false;
    }
    #endregion

    #region callback
    public void InitializeWeapon(PlayerWeapon weapon)
    {
        currentWeapon = weapon;
        maxAmmo = currentWeapon.maxAmmoNums;
        Debug.Log("currentWeapon: " + currentWeapon);
        if(currentWeapon.weaponClass == WeaponClass.Melee)
        {
            sword = Instantiate(currentWeapon.bulletType, pc.transform);
            swordAttributes = sword.GetComponent<Sword>();
            swordAttributes.pc = pc;
            swordAttributes.InitSword(currentWeapon.weaponBulletDamage, stats.playerColor);
        }
        currentAmmo = maxAmmo;
        if (stats != null)
        {
            stats.playerColor.S = 100f;
        }
    }
    public void OnWeaponChanged(PlayerWeapon weapon)
    {
        currentWeapon= weapon;
        maxAmmo = currentWeapon.maxAmmoNums;
        currentAmmo = 0;
        if (stats != null)
        {
            stats.playerColor.S = 0;
        }
        //TODO: May change reloadTime for different types of weapons
    }

    private void ResetStates()
    {
        StopAllCoroutines();

        // firing variables reset
        isFiring = false;
        isCoroutineRunning = false;
        ResetReload();
    }
    #endregion
}
