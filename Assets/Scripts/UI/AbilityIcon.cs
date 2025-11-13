using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityIcon : MonoBehaviour
{
    [Header("UI object reference")]
    public Image backgroundImage;
    public Image filledImage;
    public TextMeshProUGUI cooldownText;

    private float cooldownTime = 0;

    private void Start()
    {
        ResetAbilityUI();
        //GameManager.instance.onReset += ResetAbilityUI;
    }
    public void BindToAbility(BaseAbility ability,Sprite cooldownIcon)
    {
        ability.SetboundIcon(this);
        backgroundImage.sprite= cooldownIcon;
        filledImage.sprite= cooldownIcon;
    }

    public void StartCooldown(float time)
    {
        cooldownTime = time;
        cooldownText.gameObject.SetActive(true);
        filledImage.fillAmount = 0;

    }
    public void UpdateCooldown(float cooldownRemainTime)
    {
        filledImage.fillAmount = 1 - cooldownRemainTime / cooldownTime;
        if (cooldownRemainTime < 1)
        {
            cooldownText.SetText(cooldownRemainTime.ToString("F1"));
        }
        else
        {
            cooldownText.SetText(cooldownRemainTime.ToString("F0"));
        }
    }
    public void EndCooldown()
    {
        cooldownText.gameObject.SetActive(false);
    }
    public void ResetAbilityUI()
    {
        filledImage.fillAmount = 1;
        cooldownText.gameObject.SetActive(false);
    }
}
