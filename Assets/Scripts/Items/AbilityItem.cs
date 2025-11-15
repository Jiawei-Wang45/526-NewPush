using UnityEditor;
using UnityEngine;

public class AbilityItem : BaseItem, IInteractable
{
    [SerializeField] private PlayerAbility ability;
    [SerializeField] private bool destroyAfterPick = true;
    protected override void Awake()
    {
        base.Awake();
        spriteRenderer.sprite = ability.menuIcon;
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
        if (destroyAfterPick)
        {
            if (pc.GetInteractObject() == (IInteractable)this)
                pc.SetInteractObject(null);
            Destroy(gameObject);
        }
        

    }
}
