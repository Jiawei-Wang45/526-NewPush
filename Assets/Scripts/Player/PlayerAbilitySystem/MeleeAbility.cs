using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAbility: MonoBehaviour,IWeapon
{
    [NonSerialized] private int ENEMY_LAYER;
    [NonSerialized] private int ENEMYBULLET_LAYER;
    [NonSerialized] Transform attackPoint;
    public MeleeWeapon currentWeapon;

    [NonSerialized] private PlayerController pc;
    [NonSerialized] private Animator animator;

    [NonSerialized] private WieldEffect currentDeflectEffect = null;

    private bool isSlashing = false;
    private bool isReflecting = false;
    private bool isRecovering = false;

    public float recoveryTime = 0.10f;

    private ParryHitbox parryHitbox;
    
    
    

    private void Awake()
    {
        pc = GetComponentInParent<PlayerController>();
        attackPoint= transform.Find("PlayerAim/AttackPoint");
        animator = attackPoint.GetComponent<Animator>();
        parryHitbox = attackPoint.GetComponent<ParryHitbox>();
        ENEMY_LAYER = LayerMask.NameToLayer("Enemy");
        ENEMYBULLET_LAYER = LayerMask.NameToLayer("EnemyBullet");
        animator.SetBool("isHolding", false);
        parryHitbox.DisableParry();
    }
    public void ChangeWeapon(MeleeWeapon newWeapon)
    {
        currentWeapon = newWeapon;       
        animator.runtimeAnimatorController = newWeapon.animatorController;
        parryHitbox.SyncToSprite(newWeapon.weaponTexture);
    }
    private IEnumerator SlashCoroutine()
    {
        animator.SetTrigger("Slash");
        currentDeflectEffect =Instantiate(currentWeapon.slashEffectPrefab, attackPoint.position, attackPoint.rotation, transform);
        currentDeflectEffect.Init(WieldEffect.EffectType.Slash, currentWeapon.damage);
        yield return new WaitForSeconds(currentWeapon.slashCD);
        isSlashing = false;
    }
    
    private IEnumerator ReflectCoroutine()
    {
        //gameObject.layer = LayerMask.NameToLayer("Reflecting");
        if(isReflecting) yield break;
        isReflecting = true;
        parryHitbox.EnableParry();
        animator.SetTrigger("Parry");
        //currentDeflectEffect =Instantiate(currentWeapon.reflectEffectPrefab, attackPoint.position, attackPoint.rotation, transform);
        //currentDeflectEffect.Init(WieldEffect.EffectType.Reflect, 0);
        
    }

    private IEnumerator RecoveryCoroutine()
    {
        isRecovering = true;
        yield return new WaitForSeconds(recoveryTime);
        isRecovering = false;
    }
    #region IWeapon interface
    public void LeftMouseTriggered()
    {
        if (isSlashing || isReflecting) return;
        isSlashing = true;
        StartCoroutine(SlashCoroutine());
    }
    public void LeftMouseReleased() { }
    public void RightMouseTriggered()
    {
        if (isRecovering)
        {
            return;
        }
        animator.SetBool("isHolding", true);
        StartCoroutine(ReflectCoroutine());
    }

    public void RightMouseReleased()
    {
        if (!isReflecting) return;

        isReflecting = false;
        animator.SetBool("isHolding", false);
        parryHitbox.DisableParry();
        //if (currentDeflectEffect != null)
        //{
        //    currentDeflectEffect.DestroyItself();
        //    currentDeflectEffect = null;
        //}
        

        StartCoroutine(RecoveryCoroutine());
        
    }
    public void ReloadTriggered() { }
    #endregion IWeapon interface
}
