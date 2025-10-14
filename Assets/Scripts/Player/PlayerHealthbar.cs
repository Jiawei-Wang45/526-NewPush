using UnityEngine;
using UnityEngine.UIElements;
public class PlayerHealthbar : MonoBehaviour
{
    public GameManager gameManager;
    public PlayerStats playerStats;
    public PlayerHSLSystem hslSystem;
    public UnityEngine.UI.Image healthbarImage;

    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        playerStats = FindFirstObjectByType<PlayerControllerTest>().GetComponent<PlayerStats>();
        hslSystem = FindFirstObjectByType<PlayerControllerTest>().GetComponent<PlayerHSLSystem>();
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
    }
    
    private void Update()
    {
        if (gameManager.isPlayerAlive)
        {
            // 更新血条填充量
            healthbarImage.fillAmount = playerStats.health / playerStats.maxHealth;
            
            // 如果HSL系统存在，让HSL系统处理颜色更新
            if (hslSystem != null)
            {
                // HSL系统会自动更新颜色，这里不需要额外处理
                // 但我们可以确保healthbarImage被正确引用
                if (hslSystem.healthbarImage == null)
                {
                    hslSystem.healthbarImage = healthbarImage;
                }
            }
        }
    }
}
