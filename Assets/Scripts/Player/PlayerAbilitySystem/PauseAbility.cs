using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseAbility : BaseAbility
{
    //components
    public static PauseAbility instance;
    //pause variable
    public float activePauseDuration = 5.0f;  //different from the pause triggered by shield ghost ability, it's a stand alone pause ability
    public float activePauseCooldown = 3.0f;
    public float activePauseStrength = 20.0f; //different from the pause triggered by shield ghost ability, it's a stand alone pause ability
    //pause delegate
    public delegate void PauseStartDelegate(float pauseStrength);
    public event PauseStartDelegate OnPauseStart;

    public delegate void PauseEndDelegate();
    public event PauseEndDelegate OnPauseEnd;
    protected override void Awake()
    {
        base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GetComponent<PlayerControllerTest>().playerInput.Default.Pause.performed += OnPauseTriggered;
        GetComponent<PlayerControllerTest>().OnResetCalled += ResetStates;
    }
    private void OnPauseTriggered(InputAction.CallbackContext context)
    {
        ActivatePause(activePauseDuration, activePauseStrength, activePauseCooldown);
    }
    public void ActivatePause(float pauseDuration, float pauseStrength,float pauseCooldown)
    {
        if (isCooldown) return;
        SendAnalytics("Pause");
        StartCoroutine(PauseCoroutine(pauseDuration,pauseStrength, pauseCooldown));
        StartCoroutine(AbilityCooldownCoroutine(pauseDuration + pauseCooldown));
    }
    private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength, float pauseCooldown)
    {
        isCooldown = true;
        OnPauseStart?.Invoke(pauseStrength);
        yield return new WaitForSeconds(pauseDuration);
        OnPauseEnd?.Invoke();
        yield return new WaitForSeconds(pauseCooldown);
        isCooldown = false;
        ResetStates();
    }
    protected override void ResetStates()
    {
        base.ResetStates();
        if (isCooldown)
        {
            StopAllCoroutines();
            ResetAbilityUI();
            OnPauseEnd?.Invoke();
            isCooldown = false;
        }
    }
}
