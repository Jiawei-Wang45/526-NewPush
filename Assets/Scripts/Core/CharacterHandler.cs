using System.Collections.Generic;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHandler: MonoBehaviour
{
    [Header("UI object reference for attackingAbility")]
    public AbilityIcon attackingAbilityIcon;

    [Header("UI object reference for defenseAbility")]
    public AbilityIcon defenseAbilityIcon;

    private PlayerControllerTest pc;
    private List<PlayerWeapon> weapons;
    private GameObject attackingAbility=null;
    private GameObject defenseAbility=null;

    private void Awake()
    {
        pc = GetComponent<PlayerControllerTest>();
        weapons=new List<PlayerWeapon>();
    }
    private void Start()
    {
        weapons.Add(CharacterConfigHolder.instance.weapon);
        pc.GetComponent<FireAbility>().InitializeWeapon(weapons[0]);
        SetAttackingAbility(CharacterConfigHolder.instance.attackingAbility);
        SetDefenseAbility(CharacterConfigHolder.instance.defenseAbility);
        pc.combinationIndex = (int)CharacterConfigHolder.instance.weapon.weaponClass + (int)CharacterConfigHolder.instance.defenseAbility.abilityClass * 4;
        Debug.Log("combinationIndex: " + pc.combinationIndex);
        // Current Ability/weapon Matrix: 
        //       Pause  Dash
        // FullAuto 0     4
        // SemiAuto 1     5
        // Shotgun  2     6
        // Melee    3     7
        //
    }
    public void SetAttackingAbility(PlayerAbility ability)
    {
        if (attackingAbility!=null)
        {
            Destroy(attackingAbility);
        }
        attackingAbility = Instantiate(ability.abilityPrefab, transform);

        attackingAbilityIcon.BindToAbility(attackingAbility.GetComponent<BaseAbility>(), ability.cooldownIcon);
    }
    public void SetDefenseAbility(PlayerAbility ability)
    {
        if (defenseAbility != null)
        {
            Destroy(defenseAbility);
        }
        defenseAbility = Instantiate(ability.abilityPrefab, transform);
        defenseAbilityIcon.BindToAbility(defenseAbility.GetComponent<BaseAbility>(), ability.cooldownIcon);
    }
   
}
