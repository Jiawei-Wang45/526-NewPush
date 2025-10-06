using UnityEngine;

public class PlayerStats : MonoBehaviour
{



    public float movementSpeed = 10f;
    public float maxHealth = 100.0f;
    public float health;

    private void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if ( health<=0)
        {
            // don't destroy the camera attached to the player
            GetComponentInChildren<Camera>().transform.parent = null;
            FindFirstObjectByType<GameManager>().PlayerDead();
            Destroy(gameObject);
            
        }
    }
}
