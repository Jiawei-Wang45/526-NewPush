using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvincibleAbility : BaseAbility
{
    //components
    private ParticleSystem particleEffect;
    private ParticleSystem subParticleEffect;
    //Invincible parameter
    [Header("Invincible parameters")]
    public float speedMultiplier = 2.0f; // how many times faster during dash
    public float invincibleDuration = 3.0f; // seconds the dash lasts
    public float invincibleCooldown = 7.0f; //the overall cooldown time since the initiation 

    protected override void Awake()
    {
        base.Awake();
        particleEffect= GetComponent<ParticleSystem>();
        subParticleEffect= transform.Find("SubEmitter").GetComponent<ParticleSystem>();
    }
    private void Start()
    {
        pc.playerInput.Default.DefenseAbility.performed += OnInvincibleTriggered;
        GameManager.instance.onReset += ResetStates;
    }
    public void OnInvincibleTriggered(InputAction.CallbackContext context)
    {
        ActivateInvincible();
    }
    public void ActivateInvincible()
    {
        //if (isCooldown || !isEnabled) return;
        if (isCooldown) return;
        SendAnalytics("Invincible");
        StartCoroutine(InvincibleCoroutine());
        StartCoroutine(AbilityCooldownCoroutine(invincibleCooldown));

    }
    private IEnumerator InvincibleCoroutine()
    {
        stats.SetInvincible(true);  //update color purpose
        pc.speed = stats.movementSpeed * speedMultiplier;
        gameObject.layer = LayerMask.NameToLayer("Invincible");
        particleEffect.Play();
        float AccumulationTime = 0;
        Vector3 lastPostion=transform.position;
        while (true)
        {
            Vector3 movingDirection = transform.position - lastPostion;
            if (movingDirection.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(movingDirection.y, movingDirection.x) * Mathf.Rad2Deg;
                var shape = subParticleEffect.shape;
                shape.rotation = new Vector3(0, 0, angle-180.0f);
            }
            lastPostion = transform.position;
            AccumulationTime += Time.deltaTime;
            if (AccumulationTime > invincibleDuration)
            {
                break;
            }
            yield return null;
        }
        stats.SetInvincible(false);
        pc.speed = stats.movementSpeed;
        gameObject.layer = LayerMask.NameToLayer("Player");
        particleEffect.Stop();
    }
    protected override void ResetStates()
    {
        if (isCooldown)
        {
            base.ResetStates();
            stats.SetInvincible(false);
            pc.speed = stats.movementSpeed;
            gameObject.layer = LayerMask.NameToLayer("Player");
            particleEffect.Stop();
        }
    }
    private void OnDestroy()
    {
        pc.playerInput.Default.Invincible.performed -= OnInvincibleTriggered;
        GameManager.instance.onReset -= ResetStates;
    }
}
