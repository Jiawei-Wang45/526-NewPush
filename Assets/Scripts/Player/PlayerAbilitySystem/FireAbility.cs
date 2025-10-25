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
    private bool hasFired = false;

    //special ammo
    [Header("Special Ammo attributes")]
    public GameObject clusterBullet;

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
    public delegate void FireDelegate();
    public event FireDelegate OnFire;

    #region initialization
    private void Awake()
    {
        pc= GetComponent<PlayerControllerTest>();
        stats=GetComponent<PlayerStats>();
    }
    private void Start()
    {
        maxAmmo = currentWeapon.maxAmmoNums;
        currentAmmo = maxAmmo;
        reloadBar.SetActive(false);

        playerInput = pc.playerInput;
        //fire input binding
        playerInput.Default.Fire.performed += OnFireTriggered;
        playerInput.Default.Fire.canceled += OnFireTriggered;

        //special bullet binding
        playerInput.Default.SpecialBullet.performed += OnSpecialBulletTriggered;


        //reload input binding
        playerInput.Default.Reload.performed += OnReloadTriggered;

        //reset binding
        pc.OnResetCalled += ResetStates;
    }
    #endregion

    #region Update
    private void Update()
    {
        if (currentWeapon == null)
            return;
        if (currentWeapon.weaponType == WeaponType.Passive)
        {
            Fire();
        }
        else
        {
            if (currentWeapon.triggerType == TriggerType.Automatic)
            {
               
                if (isFiring && fireTimer >= 1 / currentWeapon.weaponFireRate)
                {
                    Fire();                  
                    fireTimer = 0;
                }
                else
                {
                    fireTimer += Time.deltaTime;
                }
            }
            else if (currentWeapon.triggerType == TriggerType.SemiAutomatic)
            {
               
                if (isFiring && !hasFired && fireTimer >= 1 / currentWeapon.weaponFireRate)
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
    }

    #endregion

    #region Fire
    private void Fire()
    {
        if (!CanFire() && !isReloading)
        {
            ActivateReload();
            return;
        }
        OnFire?.Invoke();
        ConsumeAmmo(1);
        float bulletTiltAngle = -(currentWeapon.weaponBulletInOneShot - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0; i < currentWeapon.weaponBulletInOneShot; i++)
        {
            GameObject spawnedBullet = Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation * Quaternion.Euler(0, 0, bulletTiltAngle + Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread)));
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed, currentWeapon.weaponBulletDamage,stats.playerColor);     
            bulletTiltAngle += currentWeapon.weaponFiringAngle;
        }
    }
    public bool CanFire()
    {
        return currentAmmo > 0;
    }
    public void ConsumeAmmo(int amount)
    {
        currentAmmo -= amount;
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
    #endregion
    #region Special Bullet
    private void OnSpecialBulletTriggered(InputAction.CallbackContext context)
    {
        GameObject specialBullet = Instantiate(clusterBullet, firePoint.position, firePoint.rotation);
        Bullet_Cluster bulletAttributes = specialBullet.GetComponent<Bullet_Cluster>();
        bulletAttributes.InitBullet(bulletAttributes.parentBulletSpeed, bulletAttributes.parentBulletDamage, stats.playerColor);
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
        currentAmmo = maxAmmo;
        ResetReload();
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
    public void OnWeaponChanged(PlayerWeapon newWeapon)
    {
        currentWeapon= newWeapon;
        maxAmmo = currentWeapon.maxAmmoNums;
        currentAmmo = maxAmmo;
        //TODO: May change reloadTime for different types of weapons
    }

    private void ResetStates()
    {
        StopAllCoroutines();

        // firing variables reset
        isFiring = false;
        hasFired = false;
        ResetReload();
    }
    #endregion
}
