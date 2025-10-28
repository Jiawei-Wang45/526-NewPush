using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldGhostAbility : BaseAbility
{
    //other abilities it will use
    private TracesetAbility tracesetAbility;
    private TracebackAbility tracebackAbility;
    private FireAbility fireAbility;

    //shield ghost variables
    private Vector2 savedPosition;
    private GameObject returnPointInstance;
    [Header("Shield Ghost parameters")]
    public ShieldGhost ghostType;
    public GameObject shieldShape;
    public GameObject hitboxShape;
    public GameObject returnPoint;
    public float pauseDuration = 3.0f;
    public float pauseCooldown = 3.0f;
    public float pauseStrength = 10.0f;
    protected override void Awake()
    {
        base.Awake();
        tracesetAbility = GetComponent<TracesetAbility>();
        tracebackAbility = GetComponent<TracebackAbility>();
        fireAbility=GetComponent<FireAbility>();
    }
    private void Start()
    {
        GetComponent<PlayerControllerTest>().playerInput.Default.ShieldGhost.performed += OnShieldGhostTriggered;
        GetComponent<PlayerControllerTest>().OnResetCalled += ResetStates;

    }
    private void OnShieldGhostTriggered(InputAction.CallbackContext context)
    {
        ActivateShieldGhost();
    }
    public void ActivateShieldGhost()
    {
        if (isCooldown) return;
        SendAnalytics("ShieldGhost");
        StartCoroutine(ShieldGhostCoroutine(pauseDuration));
        StartCoroutine(AbilityCooldownCoroutine(pauseDuration + pauseCooldown));
    }
    private IEnumerator ShieldGhostCoroutine(float pauseDuration)
    {
        isCooldown = true;
        // use TracesetAbility which provides player-invincible pause behavior
        tracesetAbility.ActivatePause(pauseDuration, pauseStrength, pauseCooldown);
        tracebackAbility.ActivateTrackback(pauseDuration, ghostType);
        shieldShape.SetActive(true);
        hitboxShape.SetActive(true);
        returnPointInstance = Instantiate(returnPoint, transform.position, transform.rotation);
        savedPosition = transform.position;
        yield return new WaitForSeconds(pauseDuration);
        transform.position = savedPosition;
        shieldShape.SetActive(false);
        hitboxShape.SetActive(false);
        Destroy(returnPointInstance);
        returnPointInstance = null;
        yield return new WaitForSeconds(pauseCooldown);
        AudioManager.instance.PlaySound("cooldownFinish");
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
            shieldShape.SetActive(false);
            hitboxShape.SetActive(false);
            Destroy(returnPointInstance);
            returnPointInstance= null;
            isCooldown = false;
        }
    }
}
