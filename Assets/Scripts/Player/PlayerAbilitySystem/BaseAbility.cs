using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseAbility : MonoBehaviour
{
    [SerializeField] protected Image filledImage;
    [SerializeField] protected TextMeshProUGUI cooldownText;
    protected bool isCooldown = false;
    protected virtual void Awake()
    {
        ResetAbilityUI();
    }
    protected virtual void ResetStates()
    {
        if (isCooldown)
        {
            ResetAbilityUI();
        }
    }
    protected IEnumerator AbilityCooldownCoroutine(float cooldownTime)
    {
        float cooldownRemainTime = cooldownTime;
        cooldownText.gameObject.SetActive(true);
        while (true)
        {
            cooldownRemainTime -= Time.deltaTime;
            filledImage.fillAmount = 1-cooldownRemainTime / cooldownTime;
            if (cooldownRemainTime<1)
            {
                cooldownText.SetText(cooldownRemainTime.ToString("F1"));
            }
            else
            {
                cooldownText.SetText(cooldownRemainTime.ToString("F0"));
            }
            if (cooldownRemainTime <= 0)
                break;
            yield return null;
        }
        cooldownText.gameObject.SetActive(false);
    }


    protected void ResetAbilityUI()
    {
        filledImage.fillAmount = 1;
        cooldownText.gameObject.SetActive(false);
    }
}
