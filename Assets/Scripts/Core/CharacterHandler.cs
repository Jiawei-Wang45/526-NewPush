using System.Collections.Generic;
using TMPro;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.UI;

public class CharacterHandler: MonoBehaviour
{
    [Header("UI object reference for ability1")]
    public Image backgroundImage_1;
    public Image filledImage_1;
    public TextMeshProUGUI cooldownText_1;

    [Header("UI object reference for ability2")]
    public Image backgroundImage_2;
    public Image filledImage_2;
    public TextMeshProUGUI cooldownText_2;

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
        attackingAbility.GetComponent<BaseAbility>().BindUI(backgroundImage_1, filledImage_1, cooldownText_1);
    }
    public void SetDefenseAbility(PlayerAbility ability)
    {
        if (defenseAbility != null)
        {
            Destroy(defenseAbility);
        }
        defenseAbility = Instantiate(ability.abilityPrefab, transform);
        defenseAbility.GetComponent<BaseAbility>().BindUI(backgroundImage_2, filledImage_2, cooldownText_2);
    }
   
}
