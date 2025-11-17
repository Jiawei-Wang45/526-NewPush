using System;
using System.Collections.Generic;
using TMPro;
//using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHandler: MonoBehaviour
{
    [NonSerialized] private PlayerController pc;
    //private List<RangedWeapon> weapons;
    //private GameObject attackingAbility=null;
    //private GameObject defenseAbility=null;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        //weapons=new List<RangedWeapon>();
    }
    private void Start()
    {
        if (CharacterConfigHolder.instance && CharacterConfigHolder.instance.GetConfigured())
        {
            //weapons.Add(CharacterConfigHolder.instance.weapon);
            WeaponController weaponController = FindFirstObjectByType<WeaponController>();
            if (weaponController)
            {
                weaponController.EquipNewWeapon(CharacterConfigHolder.instance.weapon);
            }
            AbilityController abilityController = FindFirstObjectByType<AbilityController>();
            if (abilityController)
            {
                abilityController.ChangeAttackingAbility(CharacterConfigHolder.instance.attackingAbility);
                abilityController.ChangeDefenseAbility(CharacterConfigHolder.instance.defenseAbility);
            }
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
    }
    //public void SetAttackingAbility(PlayerAbility ability)
    //{
    //    if (attackingAbility!=null)
    //    {
    //        Destroy(attackingAbility);
    //    }
    //    attackingAbility = Instantiate(ability.abilityPrefab, transform);

    //    attackingAbilityIcon.BindToAbility(attackingAbility.GetComponent<BaseAbility>(), ability.cooldownIcon);
    //}
    //public void SetDefenseAbility(PlayerAbility ability)
    //{
    //    if (defenseAbility != null)
    //    {
    //        Destroy(defenseAbility);
    //    }
    //    defenseAbility = Instantiate(ability.abilityPrefab, transform);
    //    defenseAbilityIcon.BindToAbility(defenseAbility.GetComponent<BaseAbility>(), ability.cooldownIcon);
    //}
   
}
