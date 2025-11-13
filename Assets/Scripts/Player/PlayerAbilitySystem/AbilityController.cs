using System;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityController : MonoBehaviour
{
    [SerializeField] private AbilityIcon attackingAbilityIcon;
    [SerializeField] private AbilityIcon defenseAbilityIcon;

    [NonSerialized] private GameObject attackingAbility;
    [NonSerialized] private GameObject defenseAbility;
    [NonSerialized] private PlayerController pc;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        
    }

    private void Start()
    {
        pc.playerInput.Default.AttackingAbility.started += OnAttackingAbilityTriggered;
        pc.playerInput.Default.DefenseAbility.started += OnDefenseAbilityTriggered;
    }

    private void OnAttackingAbilityTriggered(InputAction.CallbackContext context)
    {
        if (attackingAbility)
        {
            attackingAbility.GetComponent<BaseAbility>().ActivateAbility();
        }   
    }
    private void OnDefenseAbilityTriggered(InputAction.CallbackContext context)
    {
        if (defenseAbility)
        {
            defenseAbility.GetComponent<BaseAbility>().ActivateAbility();
        }        
    }

    public void ChangeAttackingAbility(PlayerAbility newAbility)
    {
        if (attackingAbility!=null)
        {
            Destroy(attackingAbility);
        }
        attackingAbility = Instantiate(newAbility.abilityPrefab, transform);
        attackingAbilityIcon.BindToAbility(attackingAbility.GetComponent<BaseAbility>(), newAbility.cooldownIcon);
        attackingAbilityIcon.ResetAbilityUI();
    }
    public void ChangeDefenseAbility(PlayerAbility newAbility)
    {
        if (defenseAbility!=null)
        {
            Destroy(defenseAbility);
        }
        defenseAbility = Instantiate(newAbility.abilityPrefab, transform);
        defenseAbilityIcon.BindToAbility(defenseAbility.GetComponent<BaseAbility>(), newAbility.cooldownIcon);
        defenseAbilityIcon.ResetAbilityUI();
    }
}
