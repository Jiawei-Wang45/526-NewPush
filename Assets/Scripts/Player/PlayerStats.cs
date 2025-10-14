using UnityEngine;
using System.Collections;

public class PlayerStats : MonoBehaviour
{



    public float movementSpeed = 10f;
    public float maxHealth = 100.0f;
    public float health;
    public bool isGhost = false;

    public HSLColor playerColor = new HSLColor();
    public float originalH;

    public float weaponTypeThreshold = 15f;
    public float maxAmmo = 100f;
    public float currentAmmo;

    public float reloadTime = 2f;
    private bool isReloading = false;

    public float hRecoverySpeed = 1f;
    public float hRecoveryDelay = 2f; 
    public float hRecoveryTimer = 0f;
    public bool isRecoveringH = false;
  



    private void Start()
    {
        health = maxHealth;
        currentAmmo = maxAmmo;
        originalH = playerColor.H;

        
    }
    public void TakeDamage(float damage)
    {
        health -= damage;

        playerColor.L = 50f + (health / maxHealth) * 40f;

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

    public void ChangeWeaponType(float newH, float influence)
    {
        playerColor.H = Mathf.Lerp(playerColor.H, newH, influence);
        
        // 开始H值恢复计时
        StartHRecovery();
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
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        playerColor.S = 100f;
        isReloading = false;
    }
    
    private void StartHRecovery()
    {
        hRecoveryTimer = 0f;
        isRecoveringH = true;
    }
    

    public void ResetH()
    {
        playerColor.H = originalH;
        isRecoveringH = false;
        hRecoveryTimer = 0f;

    }

    public void Reset(){
        health = maxHealth;
    }

    public bool CanFire()
    {
        return currentAmmo > 0 && !isReloading && 
               Mathf.Abs(playerColor.H - originalH) < weaponTypeThreshold;
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

    public void ConsumeAmmo(float amount)
    {
        currentAmmo -= amount;
        playerColor.S = (currentAmmo / maxAmmo) * 100f;
    }
}
