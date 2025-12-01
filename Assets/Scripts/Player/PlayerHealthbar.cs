using UnityEngine;
using UnityEngine.UIElements;
public class PlayerHealthbar : MonoBehaviour
{
    
    private PlayerStats playerStats;
    public UnityEngine.UI.Image healthbarImage;
    private Sprite[] healthSprites; 

    private void Awake()
    {
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
    }
    private void Start()
    {
        // Load all sprites from the sliced spritesheet in Resources
        healthSprites = Resources.LoadAll<Sprite>("healthbar-Sheet");

        playerStats = PlayerStats.Instance;
        playerStats.OnHealthChanged += HandleHealthChanged;
    }
    public void HandleHealthChanged()
    {
        float healthPercent = playerStats.health / playerStats.maxHealth;
        int index = Mathf.RoundToInt((1 - healthPercent) * 20);
        index = Mathf.Clamp(index, 0, 20); // Ensure index is within bounds
        healthbarImage.sprite = healthSprites[index];
        healthbarImage.fillAmount = 1; 
    }
}
