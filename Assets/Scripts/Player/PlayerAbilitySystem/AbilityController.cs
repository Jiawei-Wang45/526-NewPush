using System;
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
        pc.abilityIndex = (int)newAbility.abilityClass;
        if(pc.weaponIndex >= 0 && pc.abilityIndex >= 0)
            pc.combinationIndex = pc.weaponIndex + pc.abilityIndex * 4;
        Debug.Log("ChangeDefenseAbility: New combination index: " + pc.combinationIndex);
        // Current Ability/weapon Matrix: 
        //       Pause  Dash
        // FullAuto 0     4
        // SemiAuto 1     5
        // Shotgun  2     6
        // Melee    3     7
        //
        defenseAbilityIcon.BindToAbility(defenseAbility.GetComponent<BaseAbility>(), newAbility.cooldownIcon);
        defenseAbilityIcon.ResetAbilityUI();
    }
}
