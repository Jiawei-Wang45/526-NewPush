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
    private bool isSlashing = false;
    private bool isReflecting = false;

    private void Awake()
    {
        pc = GetComponentInParent<PlayerController>();
        attackPoint= transform.Find("PlayerAim/AttackPoint");
        animator = attackPoint.GetComponent<Animator>();
        ENEMY_LAYER = LayerMask.NameToLayer("Enemy");
        ENEMYBULLET_LAYER = LayerMask.NameToLayer("EnemyBullet");
    }
    public void ChangeWeapon(MeleeWeapon newWeapon)
    {
        currentWeapon = newWeapon;       
        animator.runtimeAnimatorController = newWeapon.animatorController;
    }
    private IEnumerator SlashCoroutine()
    {
        animator.SetTrigger("Slash");
        WieldEffect slashEffect =Instantiate(currentWeapon.slashEffectPrefab, attackPoint.position, attackPoint.rotation, transform);
        slashEffect.Init(WieldEffect.EffectType.Slash, currentWeapon.damage);
        yield return new WaitForSeconds(currentWeapon.slashCD);
        isSlashing = false;
    }
    private IEnumerator ReflectCoroutine()
    {
        //gameObject.layer = LayerMask.NameToLayer("Reflecting");
        animator.SetTrigger("Reflect");
        WieldEffect reflectEffect =Instantiate(currentWeapon.reflectEffectPrefab, attackPoint.position, attackPoint.rotation, transform);
        reflectEffect.Init(WieldEffect.EffectType.Reflect, 0);
        yield return new WaitForSeconds(currentWeapon.reflectCD);
        isReflecting = false;
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
        if (isReflecting || isSlashing) return;
        isReflecting = true;
        StartCoroutine(ReflectCoroutine());
    }
    public void ReloadTriggered() { }
    #endregion IWeapon interface
}
