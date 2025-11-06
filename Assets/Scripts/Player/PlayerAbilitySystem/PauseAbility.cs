using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseAbility : BaseAbility
{
    //pause variable
    [Header("Pause parameters")]
    public float pauseDuration = 4.0f;  
    public float pauseStrength = 20.0f;                       
    public float pauseCooldown = 20.0f;   //the overall cooldown time since the initiation                                           

    protected override void Awake()
    {
        base.Awake();
        abilityType = AbilityType.Defense;
    }
    private void Start()
    {
        pc.playerInput.Default.DefenseAbility.performed += OnPauseTriggered;
        //GameManager.instance.onReset += ResetStates;
    }
    private void OnPauseTriggered(InputAction.CallbackContext context)
    {
        ActivatePause(pauseDuration, pauseStrength, pauseCooldown);
    }
    public void ActivatePause(float pauseDuration, float pauseStrength,float pauseCooldown)
    {
        //Debug.Log($"Pause Ability enabled {isEnabled}");
        if (isCooldown) return;
        SendAnalytics("Pause");
        PauseManager.instance.RequestPause(pauseDuration, pauseStrength);
        StartCoroutine(AbilityCooldownCoroutine(pauseDuration + pauseCooldown));
    }

    // Trigger only the pause events (OnPauseStart/OnPauseEnd) without sending analytics or starting cooldown.
    // Useful for abilities that want the global pause behavior but manage their own analytics/cooldown.
    //public void TriggerPauseEvents(float pauseDuration, float pauseStrength)
    //{
    //    StartCoroutine(TriggerPauseCoroutine(pauseDuration, pauseStrength));
    //}

    //private IEnumerator TriggerPauseCoroutine(float pauseDuration, float pauseStrength)
    //{
    //    OnPauseStart?.Invoke(pauseStrength);
    //    yield return new WaitForSeconds(pauseDuration);
    //    OnPauseEnd?.Invoke();
    //}
    //private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength, float pauseCooldown)
    //{
    //    OnPauseStart?.Invoke(pauseStrength);
    //    yield return new WaitForSeconds(pauseDuration);
    //    OnPauseEnd?.Invoke();
    //    yield return new WaitForSeconds(pauseCooldown);
    //    AudioManager.instance.PlaySound("cooldownFinish");
    //    ResetStates();
    //}
    //protected override void ResetStates()
    //{
    //    if (isCooldown)
    //    {
    //        base.ResetStates();
    //    }
        
    //}
    private void OnDestroy()
    {
        pc.playerInput.Default.Pause.performed -= OnPauseTriggered;
        //GameManager.instance.onReset -= ResetStates;
    }
}
