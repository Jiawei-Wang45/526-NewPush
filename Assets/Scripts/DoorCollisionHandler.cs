using UnityEngine;

public class DoorCollisionHandler : MonoBehaviour
{
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.GetComponent<Inventory>().HasKey())
            {
                Destroy(gameObject);
            }

        }
    }
    
}