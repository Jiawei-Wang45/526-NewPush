using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvincibleAbility : BaseAbility
{
    //components
    private PlayerControllerTest pc;
    private PlayerStats stats;
    private ParticleSystem particleEffect;
    //Invincible parameter
    public float speedMultiplier = 2.0f; // how many times faster during dash
    public float InvincibleDuration = 3.0f; // seconds the dash lasts
    public float InvincibleCooldown = 3.0f; // seconds before dash can be used again

    protected override void Awake()
    {
        base.Awake();
        pc=GetComponent<PlayerControllerTest>();
        stats =GetComponent<PlayerStats>();
        particleEffect= GetComponent<ParticleSystem>();
    }
    private void Start()
    {
        GetComponent<PlayerControllerTest>().playerInput.Default.Invincible.performed += OnInvincibleTriggered;
        GetComponent<PlayerControllerTest>().OnResetCalled += ResetStates;
    }
    public void OnInvincibleTriggered(InputAction.CallbackContext context)
    {
        ActivateInvincible();
    }
    public void ActivateInvincible()
    {
        if (isCooldown) return;
        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(AbilityCooldownCoroutine(InvincibleDuration + InvincibleCooldown));

    }
    private IEnumerator InvincibleCoroutine()
    {
        isCooldown = true;
        stats.SetInvincible(true);
        pc.speed = stats.movementSpeed * speedMultiplier;
        gameObject.layer = LayerMask.NameToLayer("Invincible");
        particleEffect.Play();
        yield return new WaitForSeconds(InvincibleDuration);
        stats.SetInvincible(false);
        pc.speed = stats.movementSpeed;
        gameObject.layer = LayerMask.NameToLayer("Player");
        particleEffect.Stop();
        yield return new WaitForSeconds(InvincibleCooldown);
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
            stats.SetInvincible(false);
            pc.speed = stats.movementSpeed;
            gameObject.layer = LayerMask.NameToLayer("Player");
            particleEffect.Stop();
            isCooldown = false;
        }
    }

}
