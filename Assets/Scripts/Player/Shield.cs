using UnityEngine;

public class Shield : MonoBehaviour
{
    public float shieldHealth = 30.0f;
    public void takeDamage(float damage)
    {
        shieldHealth -= damage;
        if(shieldHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
