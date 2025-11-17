using System;
using UnityEngine;

public class WeaponItem : BaseItem, IInteractable
{
    [SerializeField] PlayerBaseWeapon weapon;

    public void InitWeapon(PlayerBaseWeapon inWeapon,Sprite inSprite)
    {
        weapon= inWeapon;
        spriteRenderer.sprite= inSprite;

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
        if (FindFirstObjectByType<WeaponController>().EquipNewWeapon(weapon))
        {
            if (pc.GetInteractObject() == (IInteractable)this)
                pc.SetInteractObject(null);
            Destroy(gameObject);
        }      
    }
}
