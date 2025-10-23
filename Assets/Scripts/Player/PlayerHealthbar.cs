using UnityEngine;
using UnityEngine.UIElements;
public class PlayerHealthbar : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerStats playerStats;
    public UnityEngine.UI.Image healthbarImage;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        playerStats = FindFirstObjectByType<PlayerControllerTest>().GetComponent<PlayerStats>();
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
    }
    //private void Update()
    //{
    //    if (gameManager.isPlayerAlive)
    //    {
    //        healthbarImage.fillAmount = playerStats.health / playerStats.maxHealth;
    //    }
        
    //}
    public void HandleHealthChanged()
    {
        healthbarImage.fillAmount = playerStats.health / playerStats.maxHealth;
    }
}
