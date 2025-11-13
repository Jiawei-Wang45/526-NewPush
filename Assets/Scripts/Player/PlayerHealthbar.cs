using UnityEngine;
using UnityEngine.UIElements;
public class PlayerHealthbar : MonoBehaviour
{
    
    private PlayerStats playerStats;
    public UnityEngine.UI.Image healthbarImage;
    private void Awake()
    {
        healthbarImage = GetComponent<UnityEngine.UI.Image>();
    }
    private void Start()
    {
        playerStats = PlayerStats.Instance;
        playerStats.OnHealthChanged += HandleHealthChanged;
    }
    public void HandleHealthChanged()
    {
        healthbarImage.fillAmount = playerStats.health / playerStats.maxHealth;
    }
}
