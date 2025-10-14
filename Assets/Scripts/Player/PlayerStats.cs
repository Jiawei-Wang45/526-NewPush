using UnityEngine;

public class PlayerStats : MonoBehaviour
{



    public float movementSpeed = 10f;
    public float maxHealth = 100.0f;
    public float health;
    public bool isGhost = false;

    private void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
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
    
    public void Reset()
    {
        health = maxHealth;
    }
}
