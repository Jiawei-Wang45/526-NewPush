using UnityEngine;

public class DoorTemp : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int EnemiesToKill = 25;

    public void resetEnemiesToKill()
    {
        EnemiesToKill = 25;
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (FindFirstObjectByType<GameManager>().GetSpawnCompleted()){
            if(EnemiesToKill == 0){
                Destroy(gameObject);
            }
        }
    }
}
