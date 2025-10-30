using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseAbility : MonoBehaviour
{
    [Header("cooldown UI parameters")]
    [SerializeField] protected Image filledImage;
    [SerializeField] protected TextMeshProUGUI cooldownText;
    protected bool isCooldown = false;
    private Coroutine disableRoutine = null;

    protected bool isEnabled = true;
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
            filledImage.fillAmount = 1 - cooldownRemainTime / cooldownTime;
            if (cooldownRemainTime < 1)
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
        isCooldown = false;
    }

    protected void ResetAbilityUI()
    {
        filledImage.fillAmount = 1;
        cooldownText.gameObject.SetActive(false);
    }

    public void DisableAbility(float duration)
    {
        if (disableRoutine != null) StopCoroutine(disableRoutine);
        disableRoutine = StartCoroutine(DisableCoroutine(duration));
    }
    
    protected IEnumerator DisableCoroutine(float cooldownTime)
    {
        isEnabled = false;
        yield return new WaitForSeconds(cooldownTime);
        isEnabled = true;
        disableRoutine = null;
    }


    protected void SendAnalytics(string abilityType)
    {
        // Analytics temporarily disabled: do not send ability-use events with wave data
        // if (PlayerControllerTest.instance != null && PlayerControllerTest.instance.sendToGoogle != null)
        // {
        //     GameManager gm = FindFirstObjectByType<GameManager>();
        //     int waveToSend = gm != null ? gm.CurrentWave : 0;
        //     PlayerControllerTest.instance.sendToGoogle.SendAbilityUse(PlayerControllerTest.instance.transform.position, waveToSend, abilityType);
        // }
    } 
}
