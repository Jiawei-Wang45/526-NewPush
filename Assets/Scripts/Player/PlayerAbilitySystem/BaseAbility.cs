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
    //[Header("UI object reference")]
    //public Image backgroundImage;
    //public Image filledImage;
    //public TextMeshProUGUI cooldownText;
    //[Header("UI updating assets")]
    //public Sprite cooldownIcon;
    [Header("Ability parameter")]
    protected bool isCooldown = false;

    //ability Icon 
    private AbilityIcon boundIcon;
    ////cooldown delegate
    //public delegate void OnCooldownStartDelegate(float cooldownTime);
    //public event OnCooldownStartDelegate onCooldownStart;

    //public delegate void OnCooldownUpdate(float ratio);
    //public event OnCooldownUpdate onCooldownUpdate;


    //private Coroutine disableRoutine = null;

    //protected bool isEnabled = true;
    //public void BindUI(Image inBackgroundImage, Image inFilledImage, TextMeshProUGUI inCooldownText)
    //{
    //    backgroundImage = inBackgroundImage;
    //    filledImage= inFilledImage;
    //    cooldownText = inCooldownText;
    //    // initialize the cooldown Icon, since different ability contains different Icon
    //    backgroundImage.sprite = cooldownIcon;
    //    filledImage.sprite = cooldownIcon;
    //    ResetAbilityUI();
    //}
    public void SetboundIcon(AbilityIcon Icon)
    {
        boundIcon = Icon;
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
        //ResetAbilityUI();
        isCooldown = false;
    }
    protected IEnumerator AbilityCooldownCoroutine(float cooldownTime)
    {
        isCooldown = true;
        //cooldownText.gameObject.SetActive(true);
        boundIcon.StartCooldown(cooldownTime);
        float cooldownRemainTime = cooldownTime;
        while (true)
        {
            cooldownRemainTime -= Time.deltaTime;
            boundIcon.UpdateCooldown(cooldownRemainTime);
            if (cooldownRemainTime <= 0)
                break;
            yield return null;
        }
        boundIcon.EndCooldown();
        AudioManager.instance.PlaySound("cooldownFinish");
        isCooldown = false;
    }

    //protected void ResetAbilityUI()
    //{
    //    filledImage.fillAmount = 1;
    //    cooldownText.gameObject.SetActive(false);
    //}

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
        // Analytics temporarily disabled: do not send ability-use events with wave data
        // if (PlayerControllerTest.instance != null && PlayerControllerTest.instance.sendToGoogle != null)
        // {
        //     GameManager gm = FindFirstObjectByType<GameManager>();
        //     int waveToSend = gm != null ? gm.CurrentWave : 0;
        //     PlayerControllerTest.instance.sendToGoogle.SendAbilityUse(PlayerControllerTest.instance.transform.position, waveToSend, abilityType);
        // }
    } 
}
