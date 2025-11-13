using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InvincibleAbility : BaseAbility
{
    //components
    [NonSerialized] private ParticleSystem particleEffect;
    [NonSerialized] private ParticleSystem subParticleEffect;
    [NonSerialized] private SpriteRenderer parentSprite;
    //Invincible parameter
    [Header("Invincible parameters")]
    public float speedMultiplier = 2.0f; // how many times faster during dash
    public float invincibleDuration = 3.0f; // seconds the dash lasts
    public float invincibleCooldown = 7.0f; //the overall cooldown time since the initiation 
    private float cachedSpeed;
    protected override void Awake()
    {
        base.Awake();
        abilityType = AbilityType.Defense;
        cachedSpeed = pc.speed;
        particleEffect = GetComponent<ParticleSystem>();
        subParticleEffect= transform.Find("SubEmitter").GetComponent<ParticleSystem>();
        parentSprite = GetComponentInParent<SpriteRenderer>();
    }
    //private void Start()
    //{
    //    pc.playerInput.Default.DefenseAbility.performed += OnInvincibleTriggered;
    //    //GameManager.instance.onReset += ResetStates;
    //}
    public override void ActivateAbility()
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
        pc.GetComponent<PlayerStats>().SetInvincible(true);
        if(pc.combinationIndex == 4)
        {
            pc.GetComponent<FireAbility>().SetAmmoToMax();
        }
        pc.speed = cachedSpeed * speedMultiplier;
        pc.gameObject.layer = LayerMask.NameToLayer("Invincible");
        particleEffect.Play();
        ChangeVisuality(0.5f);
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
        pc.speed = cachedSpeed;
        pc.gameObject.layer = LayerMask.NameToLayer("Player");
        particleEffect.Stop();
        ChangeVisuality(1);
        pc.GetComponent<PlayerStats>().SetInvincible(false);
        if(pc.combinationIndex == 4)
        {
            pc.GetComponent<FireAbility>().SetAmmoToMax();
        }
    }
    //protected override void ResetStates()
    //{
    //    if (isCooldown)
    //    {
    //        base.ResetStates();
    //        stats.SetInvincible(false);
    //        pc.speed = stats.movementSpeed;
    //        gameObject.layer = LayerMask.NameToLayer("Player");
    //        particleEffect.Stop();
    //    }
    //}
    private void ChangeVisuality(float alpha)
    {
        Color color = parentSprite.color;
        color.a = alpha;
        parentSprite.color = color;
    }
    private void OnDestroy()
    {
        pc.speed = cachedSpeed;
        pc.gameObject.layer = LayerMask.NameToLayer("Player");
        particleEffect.Stop();
        ChangeVisuality(1);
        pc.GetComponent<PlayerStats>().SetInvincible(false);
    }
}
