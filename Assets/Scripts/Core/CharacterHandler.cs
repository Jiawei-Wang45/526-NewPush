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
        pc.speed = 16.0f;
        //weapons=new List<RangedWeapon>();
    }
    private void Start()
    {
        if (CharacterConfigHolder.instance && CharacterConfigHolder.instance.GetConfigured())
        {
            WeaponController weaponController = FindFirstObjectByType<WeaponController>();
            if (weaponController)
            {
                // Load all weapons from the saved list
                if (CharacterConfigHolder.instance.weaponList != null && CharacterConfigHolder.instance.weaponList.Count > 0)
                {
                    PlayerBaseWeapon currentWeapon = CharacterConfigHolder.instance.weapon;
                    
                    // Equip all weapons (they will be added to slots automatically)
                    foreach (PlayerBaseWeapon weapon in CharacterConfigHolder.instance.weaponList)
                    {
                        if (weapon != null)
                        {
                            weaponController.EquipNewWeapon(weapon);
                        }
                    }
                    
                    // Switch to the previously equipped weapon if it exists
                    if (currentWeapon != null)
                    {
                        int weaponSlotIndex = weaponController.FindWeaponSlotIndex(currentWeapon);
                        if (weaponSlotIndex >= 0)
                        {
                            weaponController.ChangeWeapon(weaponSlotIndex, true);
                        }
                        else
                        {
                            Debug.LogWarning($"[CharacterHandler] Could not find saved current weapon '{currentWeapon.name}' in loaded weapons. Keeping last equipped weapon.");
                        }
                    }
                }
                // Backward compatibility: if weaponList is empty but weapon is set
                else if (CharacterConfigHolder.instance.weapon != null)
                {
                    weaponController.EquipNewWeapon(CharacterConfigHolder.instance.weapon);
                }
            }
            AbilityController abilityController = FindFirstObjectByType<AbilityController>();
            if (abilityController)
            {
                abilityController.ChangeAttackingAbility(CharacterConfigHolder.instance.attackingAbility);
                abilityController.ChangeDefenseAbility(CharacterConfigHolder.instance.defenseAbility);
            }
            
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
