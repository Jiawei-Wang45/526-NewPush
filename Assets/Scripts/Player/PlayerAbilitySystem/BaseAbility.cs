using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseAbility : MonoBehaviour
{
    //components
    protected PlayerControllerTest pc;
    protected PlayerStats stats;

    //cooldown UI parameters
    [Header("UI object reference")]
    public Image backgroundImage;
    public Image filledImage;
    public TextMeshProUGUI cooldownText;
    [Header("UI updating assets")]
    public Sprite cooldownIcon;
    [Header("Ability parameter")]
    protected bool isCooldown = false;
    //private Coroutine disableRoutine = null;

    //protected bool isEnabled = true;
    public void BindUI(Image inBackgroundImage, Image inFilledImage, TextMeshProUGUI inCooldownText)
    {
        backgroundImage = inBackgroundImage;
        filledImage= inFilledImage;
        cooldownText = inCooldownText;
        // initialize the cooldown Icon, since different ability contains different Icon
        backgroundImage.sprite = cooldownIcon;
        filledImage.sprite = cooldownIcon;
        ResetAbilityUI();
    }
    //protected virtual void Awake()
    //{  
    //    ResetAbilityUI();
    //}
    protected virtual void Awake()
    {
        pc = GetComponentInParent<PlayerControllerTest>();
        stats = GetComponentInParent<PlayerStats>();
    }
    protected virtual void ResetStates()
    {
        StopAllCoroutines();
        ResetAbilityUI();
        isCooldown = false;
    }
    protected IEnumerator AbilityCooldownCoroutine(float cooldownTime)
    {
        isCooldown = true;
        cooldownText.gameObject.SetActive(true);
        float cooldownRemainTime = cooldownTime;
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
        AudioManager.instance.PlaySound("cooldownFinish");
        isCooldown = false;
    }

    protected void ResetAbilityUI()
    {
        filledImage.fillAmount = 1;
        cooldownText.gameObject.SetActive(false);
    }

    //public void DisableAbility(float duration)
    //{
    //    if (disableRoutine != null) StopCoroutine(disableRoutine);
    //    disableRoutine = StartCoroutine(DisableCoroutine(duration));
    //}
    
    //protected IEnumerator DisableCoroutine(float cooldownTime)
    //{
    //    isEnabled = false;
    //    yield return new WaitForSeconds(cooldownTime);
    //    isEnabled = true;
    //    disableRoutine = null;
    //}


    protected void SendAnalytics(string abilityType)
    {
        if (PlayerControllerTest.instance != null && PlayerControllerTest.instance.sendToGoogle != null)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            int waveToSend = gm != null ? gm.CurrentWave : 0;
            PlayerControllerTest.instance.sendToGoogle.SendAbilityUse(PlayerControllerTest.instance.transform.position, waveToSend, abilityType);
        }
    } 
}
