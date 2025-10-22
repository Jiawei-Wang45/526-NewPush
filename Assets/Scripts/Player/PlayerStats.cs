using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public float movementSpeed = 10f;
    public float maxHealth = 5.0f;
    public float health;
    public bool isGhost = false;

    // Invincibility flag for testing or power-ups
    public bool isInvincible = false;

    public HSLColor playerColor = new HSLColor();
    public float originalH;

    public float weaponTypeThreshold = 15f;
    public int maxAmmo;
    public int currentAmmo;

    public float reloadTime = 1.5f;
    private bool isReloading = false;

    public float hRecoverySpeed = 1f;
    public float hRecoveryDelay = 2f; 
    public float hRecoveryTimer = 0f;
    public bool isRecoveringH = false;

    public GameObject reloadBar;
    public GameObject handle;
    public float targetOffsetX;

    private void Start()
    {
        health = maxHealth;
        originalH = playerColor.H;
        if (!isGhost)
        {
            maxAmmo = GetComponent<PlayerControllerTest>().currentWeapon.maxAmmoNums;
            currentAmmo = maxAmmo;
            reloadBar.SetActive(false);
        }
        
        
    }
    public void TakeDamage(float damage, HSLColor bulletColor)
    {
        if (isInvincible) return;

        health = Mathf.Clamp(health - damage,0, maxHealth);

        playerColor.L = 50f + (1-(health / maxHealth)) * 40f;
        playerColor.H = Mathf.Lerp(playerColor.H, bulletColor.H, 0.01f);
        // start to recover the Hvalue
        StartHRecovery();
        if (health <= 0)
        {
            if (!isGhost)
            {
                FindFirstObjectByType<GameManager>().PlayerDestroyed();
            }
            else
            {
                gameObject.SetActive(false);
            }

        }
    }

    public void StartReload()
    {
        if (!isReloading)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        reloadBar.SetActive(true);
        float accumulateTime = 0;
        while(true)
        {
            setHandleOffsetX(accumulateTime / reloadTime);
            accumulateTime += Time.deltaTime;
            if (accumulateTime > reloadTime)
                break;
            yield return null;
        }
        currentAmmo = maxAmmo;
        playerColor.S = 100f;
        ResetReload();
    }
    public void ResetReload()
    {
        reloadBar.SetActive(false);
        isReloading = false;
    }
    private void setHandleOffsetX(float percent)
    {
        Vector3 handleLocalPos = handle.transform.localPosition;
        handleLocalPos.x = (percent-0.5f) * targetOffsetX;
        handle.transform.localPosition = handleLocalPos;
    }
    private void StartHRecovery()
    {
        hRecoveryTimer = 0f;
        isRecoveringH = true;
    }
    

    public void ResetH()
    {
        playerColor.H = originalH;
        playerColor.L = 50f;
        isRecoveringH = false;
        hRecoveryTimer = 0f;


    }

    public void Reset(){
        health = maxHealth;
        ResetH();
        ResetReload();
    }

    public bool CanFire()
    {
        return currentAmmo > 0 && !isReloading;
    }


    private void UpdateHRecovery()
    {
        if (isRecoveringH)
        {
            hRecoveryTimer += Time.deltaTime;

            if (hRecoveryTimer >= hRecoveryDelay)
            {

                float recoveryProgress = (hRecoveryTimer - hRecoveryDelay) * hRecoverySpeed;
                recoveryProgress = Mathf.Clamp01(recoveryProgress);
                

                float lerpSpeed = hRecoverySpeed * Time.deltaTime;
                playerColor.H = Mathf.Lerp(playerColor.H, originalH, lerpSpeed);
                

                if (Mathf.Abs(playerColor.H - originalH) < 0.5f)
                {
                    playerColor.H = originalH;
                    isRecoveringH = false;
                    hRecoveryTimer = 0f;
                    Debug.Log("H back to normal" + originalH);
                }
            }
        }
    }

    public void ConsumeAmmo(int amount)
    {
        currentAmmo -= amount;
        playerColor.S = (currentAmmo / maxAmmo) * 100f;
    }
    public void OnWeaponChanged(PlayerWeapon newWeapon)
    {
        maxAmmo = newWeapon.maxAmmoNums;
        currentAmmo = maxAmmo;
        //TODO: May change reloadTime for different types of weapons
    }
}
