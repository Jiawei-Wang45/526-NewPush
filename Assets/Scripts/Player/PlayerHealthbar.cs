using UnityEngine;
using UnityEngine.UIElements;
public class PlayerHealthbar : MonoBehaviour
{
    
    public PlayerStats playerStats;
    public UnityEngine.UI.Image healthbarImage;
    private void Awake()
    {
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
    }
    private void Start()
    {
        playerStats = PlayerControllerTest.instance.stats;
        playerStats.OnHealthChanged += HandleHealthChanged;
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
