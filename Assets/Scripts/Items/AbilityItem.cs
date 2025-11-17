using UnityEditor;
using UnityEngine;

public class AbilityItem : BaseItem, IInteractable
{
    [SerializeField] private PlayerAbility ability;
    public void InitAbility(PlayerAbility inAbility)
    {
        ability = inAbility;
        spriteRenderer.sprite = ability.cooldownIcon;
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            pc.SetInteractObject(this);
        }
    }
    public override void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (pc.GetInteractObject() == (IInteractable) this)
                pc.SetInteractObject(null);
        }
    }
    public virtual void Interact()
    {
        switch (ability.abilityClass)
        {
            case AbilityClass.Pause:
            case AbilityClass.Dash:
                FindFirstObjectByType<AbilityController>().ChangeDefenseAbility(ability);
                break;
            case AbilityClass.Attack:
                FindFirstObjectByType<AbilityController>().ChangeAttackingAbility(ability);
                break;
        }
        if (pc.GetInteractObject() == (IInteractable)this)
            pc.SetInteractObject(null);
        Destroy(gameObject);


    }
}
