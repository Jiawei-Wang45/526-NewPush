using UnityEngine;

public class PlayerStats : MonoBehaviour
{



    public float movementSpeed = 10f;
    public float maxHealth = 100.0f;
    public float health;
    public int lives = 3;
    public bool isGhost = false;

    private void Start()
    {
        health = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health<=0)
        {
            if (!isGhost)
            {
                if(lives > 0) {
                    FindFirstObjectByType<GameManager>().ResetWithGhost();
                    health = maxHealth;
                    lives--;
                } else {
                    // don't destroy the camera attached to the player
                    GetComponentInChildren<Camera>().transform.parent = null;
                    Destroy(gameObject);
                    FindFirstObjectByType<GameManager>().PlayerDead();
                }
                
            } else
            {
                gameObject.SetActive(false);
            }
            
        }
    }
}
