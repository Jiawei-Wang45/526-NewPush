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
