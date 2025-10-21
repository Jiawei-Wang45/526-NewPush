using UnityEngine;

public class Shield : MonoBehaviour, IDamagable
{
    public float shieldHealth = 30.0f;

    public void TakeDamage(float damage, HSLColor bulletColor)
    {
        shieldHealth -= damage;
        Debug.Log("Shield health: " + shieldHealth);
        if(shieldHealth <= 0)
        {
            Bullet_Default[] bullets = FindObjectsByType<Bullet_Default>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Bullet_Default b in bullets)
            {
                Destroy(b.gameObject);
            }

            Destroy(gameObject);
        }
    }
}
