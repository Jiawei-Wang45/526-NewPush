using System;
using UnityEngine;

public class WeaponItem : BaseItem, IInteractable
{
    [SerializeField] PlayerBaseWeapon weapon;
    [SerializeField] private bool destroyAfterPick = true;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer.sprite = weapon.weaponTexture;
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer==LayerMask.NameToLayer("Player"))
        {
            pc.SetInteractObject(this);
        }
    }
    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (pc.GetInteractObject()==(IInteractable)this)
                pc.SetInteractObject(null);
        }
    }
    public virtual void Interact()
    {
        FindFirstObjectByType<WeaponController>().ChangeWeapon(weapon);
        if (destroyAfterPick )
        {
            if (pc.GetInteractObject() == (IInteractable)this)
                pc.SetInteractObject(null);
            Destroy(gameObject);
        }
        
    }
}
