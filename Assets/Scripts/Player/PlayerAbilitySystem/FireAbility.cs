using System;
using System.Collections;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class FireAbility: MonoBehaviour,IWeapon
{
    [NonSerialized] private PlayerStats stats;
    [NonSerialized] private PlayerController pc;
    [NonSerialized] DashAbility dashAbility;
    // firing variables
    [Header("Basic Firing parameters")]
    [NonSerialized] private Transform attackPoint;
    public BaseWeapon currentWeapon;
    private bool isFiring = false;
    private bool isCoroutineRunning = false;
    public GameObject laser;
    private GameObject sword;
    private Sword swordAttributes;
    //reload variables
    [Header("Reloading parameters")]
    private int currentAmmo;
    private bool isReloading = false;
    //reload UI
    [Header("Reload UI settings")]
    public GameObject reloadBar;
    public GameObject backgroundBar;
    public GameObject handle;
    private float targetOffsetX;

    #region initialization
    private void Awake()
    {
        pc= GetComponent<PlayerController>();
        stats=GetComponent<PlayerStats>();
        dashAbility= GetComponent<DashAbility>();
        attackPoint= transform.Find("PlayerAim/AttackPoint");
        targetOffsetX = backgroundBar.transform.localScale.x;
    }
    private void Start()
    {
        reloadBar.SetActive(false);
    }
    #endregion

    #region Update
    private void Update()
    {
        if(currentWeapon != null && currentWeapon.weaponClass == WeaponClass.Melee && sword != null && !swordAttributes.Swinging())
        {
            sword.transform.rotation = attackPoint.rotation;
        }
    }
    #endregion

    #region Fire
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
                yield return new WaitForSeconds(currentWeapon.weaponFireInterval);
                isCoroutineRunning = false;
                break;
            case WeaponClass.FullAuto:
                isCoroutineRunning = true;
                Fire();
                float fireIntervalMultiplier = (pc.combinationIndex == 0 && PauseManager.instance.isPausing) ? 0.3f : 1.0f;
                yield return new WaitForSeconds(currentWeapon.weaponFireInterval * fireIntervalMultiplier);
                isCoroutineRunning = false;
                if (isFiring)
                {
                    StartCoroutine(FireCoroutine());
                }
                break;
            case WeaponClass.Melee:
                isCoroutineRunning = true;
                if (pc.combinationIndex == 7)
                {
                    // Dash forward with invincible effect
                    Vector2 knockbackDirection = (attackPoint.rotation * Vector2.right);
                    pc.AddForcePlayer(knockbackDirection * 10.0f);
                }
                Swing(currentWeapon.weaponFireInterval / 2.0f);
                yield return new WaitForSeconds(currentWeapon.weaponFireInterval);
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
                    Quaternion laserRotation = attackPoint.rotation * Quaternion.Euler(0, 0, 90.0f+ bulletTiltAngle + UnityEngine.Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread));
                    
                    GameObject spawnedLaser = Instantiate(laser, attackPoint.position - laserRotation * Vector2.up * 50.0f, laserRotation);
                    Bullet_Laser laserAttributes = spawnedLaser.GetComponent<Bullet_Laser>();
                    laserAttributes.InitBullet(0.0f, bulletDamage*5.0f);
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
            GameObject spawnedBullet = Instantiate(currentWeapon.bulletType, attackPoint.position, attackPoint.rotation * Quaternion.Euler(0, 0, bulletTiltAngle + UnityEngine.Random.Range(-currentWeapon.weaponBulletSpread, currentWeapon.weaponBulletSpread)));
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();     
            bulletAttributes.InitBullet(bulletSpeed, bulletDamage, bounceCount);
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
            Vector2 knockbackDirection = -(attackPoint.rotation * Vector2.right);
            pc.AddForcePlayer(knockbackDirection * 20.0f);
        }
        if(pc.combinationIndex == 2)
        {
            PauseManager.instance.extendDuration += 0.1f;
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
    }
    #endregion Fire
    #region Reload
    public void ActivateReload()
    {
        if (isReloading) return;
        isReloading = true;
        StartCoroutine(ReloadCoroutine());
    
    }
    

    public void SetAmmoToMax()
    {
        currentAmmo = currentWeapon.maxAmmoNums;
    }

    private IEnumerator ReloadCoroutine()
    {
        reloadBar.SetActive(true);
        float accumulateTime = 0;
        while (true)
        {
            setHandleOffsetX(accumulateTime / currentWeapon.reloadTime);
            accumulateTime += Time.deltaTime;
            if (accumulateTime > currentWeapon.reloadTime)
                break;
            yield return null;
        }
        SetAmmoToMax();
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
    #endregion Reload

    #region callback
    public void InitializeWeapon(BaseWeapon weapon)
    {
        currentWeapon = weapon;
        Debug.Log("currentWeapon: " + currentWeapon);
        if(currentWeapon.weaponClass == WeaponClass.Melee)
        {
            sword = Instantiate(currentWeapon.bulletType, pc.transform);
            swordAttributes = sword.GetComponent<Sword>();
            swordAttributes.pc = pc;
            swordAttributes.InitSword(currentWeapon.weaponBulletDamage);
        }
    }
    public void ChangeWeapon(BaseWeapon newWeapon)
    {
        currentWeapon= newWeapon;
        SetAmmoToMax();
    }

    //private void ResetStates()
    //{
    //    StopAllCoroutines();

    //    // firing variables reset
    //    isFiring = false;
    //    isCoroutineRunning = false;
    //    ResetReload();
    //}
    #endregion callback
    #region IWeapon Interface
    public void LeftMouseTriggered()
    {
        if (currentWeapon == null) return;
        isFiring = true;
        StartCoroutine(FireCoroutine());
    }
    public void LeftMouseReleased()
    {
        if (currentWeapon == null) return;
        isFiring = false;
    }
    public void RightMouseTriggered()
    {
        dashAbility.ActivateDash();
    }
    public void ReloadTriggered()
    {
        ActivateReload();
    }

    #endregion IWeapon Interface


}
