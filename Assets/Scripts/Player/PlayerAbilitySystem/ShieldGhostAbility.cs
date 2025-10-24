using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldGhostAbility : BaseAbility
{
    //other abilities it will use
    private PauseAbility pauseAbility;
    private TracebackAbility tracebackAbility;
    private FireAbility fireAbility;
    //shield ghost variables
    private Vector2 savedPosition;
    public ShieldGhost ghostType;
    public GameObject shieldShape;
    public GameObject hitboxShape;
    public GameObject returnPoint;
    private GameObject returnPointInstance;
    public float pauseDuration = 3.0f;
    public float pauseCooldown = 3.0f;
    public float pauseStrength = 10.0f;
    protected override void Awake()
    {
        base.Awake();
        pauseAbility = GetComponent<PauseAbility>();
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
        StartCoroutine(ShieldGhostCoroutine(pauseDuration));
        StartCoroutine(AbilityCooldownCoroutine(pauseDuration + pauseCooldown));
    }
    private IEnumerator ShieldGhostCoroutine(float pauseDuration)
    {
        isCooldown = true;
        pauseAbility.ActivatePause(pauseDuration, pauseStrength, pauseCooldown);
        tracebackAbility.ActivateTrackback(pauseDuration, ghostType, fireAbility.currentWeapon);
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
