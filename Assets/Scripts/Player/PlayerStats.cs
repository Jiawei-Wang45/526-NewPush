using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public PlayerControllerTest pc;
    public float movementSpeed = 10f;
    public float maxHealth = 5.0f;
    public float health;
    public bool isGhost = false;

    // Invincibility flag for testing or power-ups
    public bool isInvincible = false;
    public bool preventDamage = false;

    public HSLColor playerColor = new HSLColor();
    public float originalH=0;

    public float reloadTime = 1.5f;

    public float hRecoverySpeed = 1f;
    public float hRecoveryDelay = 2f; 
    public float hRecoveryTimer = 0f;
    public bool isRecoveringH = false;


    public delegate void HealthChangedDelegate();
    public event HealthChangedDelegate OnHealthChanged;
    #region routine functions
    private void Awake()
    {
        pc= GetComponent<PlayerControllerTest>();
    }
    private void Start()
    {
        ResetStates();
        //if (!isGhost)
        //{
        //    pc.OnResetCalled += ResetStates;
        //}
        GameManager.instance.onReset += ResetStates;
    }
    private void Update()
    {
        UpdatePlayerColor();
    }
    #endregion routine functions

    #region damage, health functions
    public void TakeDamage(float damage, HSLColor bulletColor)
    {
        if (preventDamage) return;
        ChangeHealth(Mathf.Clamp(health - damage, 0, maxHealth));
        playerColor.L = 50f + (1 - (health / maxHealth)) * 25f;
        /*
        playerColor.H = Mathf.Lerp(playerColor.H, bulletColor.H, 0.01f);
        // start to recover the Hvalue
        StartHRecovery();
        */
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
    public void ChangeHealth(float newHealth)
    {
        health = newHealth;
        OnHealthChanged?.Invoke();
    }
    #endregion damage, health functions

    #region color functions
    private void UpdatePlayerColor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color baseColor = playerColor.ToRGB();
            if (isInvincible)
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
    }

    public void SetInvincible(bool Invincible)
    {
        isInvincible = Invincible;
    }
    #endregion color functions

    #region reset functions
    public void ResetH()
    {
        playerColor.H = originalH;
        playerColor.L = 50f;
        isRecoveringH = false;
        hRecoveryTimer = 0f;


    }
    public void ResetStates()
    {
        ChangeHealth(maxHealth);
        ResetH();
        
    }
    #endregion reset functions
    //private void UpdateHRecovery()
    //{
    //    if (isRecoveringH)
    //    {
    //        hRecoveryTimer += Time.deltaTime;

    //        if (hRecoveryTimer >= hRecoveryDelay)
    //        {

    //            float recoveryProgress = (hRecoveryTimer - hRecoveryDelay) * hRecoverySpeed;
    //            recoveryProgress = Mathf.Clamp01(recoveryProgress);


    //            float lerpSpeed = hRecoverySpeed * Time.deltaTime;
    //            playerColor.H = Mathf.Lerp(playerColor.H, originalH, lerpSpeed);


    //            if (Mathf.Abs(playerColor.H - originalH) < 0.5f)
    //            {
    //                playerColor.H = originalH;
    //                isRecoveringH = false;
    //                hRecoveryTimer = 0f;
    //                Debug.Log("H back to normal" + originalH);
    //            }
    //        }
    //    }
    //}
    //private void StartHRecovery()
    //{
    //    hRecoveryTimer = 0f;
    //    isRecoveringH = true;
    //}

}
