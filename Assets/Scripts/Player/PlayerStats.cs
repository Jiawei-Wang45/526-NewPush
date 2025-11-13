using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;
    public float maxHealth = 5.0f;
    public float health;

    // Invincibility flag for testing or power-ups
    public bool isInvincible = false;
    public bool preventDamage = false;

    public delegate void HealthChangedDelegate();
    public event HealthChangedDelegate OnHealthChanged;
    #region routine functions
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    private void Start()
    {
        ChangeHealth(maxHealth);
        //GameManager.instance.onReset += ResetStates;
    }
    #endregion routine functions

    #region damage, health functions
    public void TakeDamage(float damage)
    {
        if (preventDamage) return;
        ChangeHealth(Mathf.Clamp(health - damage, 0, maxHealth));
        if (health <= 0)
        {
            FindFirstObjectByType<GameManager>().PlayerDestroyed();
        }
    }
    public void ChangeHealth(float newHealth)
    {
        health = newHealth;
        OnHealthChanged?.Invoke();
    }
    #endregion damage, health functions

    public void SetInvincible(bool Invincible)
    {
        isInvincible = Invincible;
    }

}
