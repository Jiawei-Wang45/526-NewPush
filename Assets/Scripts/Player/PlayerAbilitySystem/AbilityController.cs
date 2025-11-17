using System;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityController : MonoBehaviour
{
    [SerializeField] private AbilityIcon attackingAbilityIcon;
    [SerializeField] private AbilityIcon defenseAbilityIcon;
    [SerializeField] private AbilityItem droppedAbilityPrefab;
    [NonSerialized] private GameObject attackingAbility; 
    [NonSerialized] private GameObject defenseAbility;
    [NonSerialized] private PlayerAbility cachedAttackingAbility;
    [NonSerialized] private PlayerAbility cachedDefenseAbility;
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
            AbilityItem droppedAbility = Instantiate(droppedAbilityPrefab, GetRandomPosAroundPlayer(), Quaternion.identity);
            droppedAbility.InitAbility(cachedAttackingAbility);
            Destroy(attackingAbility);
        }
        attackingAbility = Instantiate(newAbility.abilityPrefab, transform);
        attackingAbilityIcon.BindToAbility(attackingAbility.GetComponent<BaseAbility>(), newAbility.cooldownIcon);
        attackingAbilityIcon.ResetAbilityUI();
        cachedAttackingAbility = newAbility;
    }
    public void ChangeDefenseAbility(PlayerAbility newAbility)
    {
        if (defenseAbility!=null)
        {
            AbilityItem droppedAbility = Instantiate(droppedAbilityPrefab, GetRandomPosAroundPlayer(), Quaternion.identity);
            droppedAbility.InitAbility(cachedDefenseAbility);
            Destroy(defenseAbility);
        }
        defenseAbility = Instantiate(newAbility.abilityPrefab, transform);
        defenseAbilityIcon.BindToAbility(defenseAbility.GetComponent<BaseAbility>(), newAbility.cooldownIcon);
        defenseAbilityIcon.ResetAbilityUI();
        cachedDefenseAbility = newAbility;
    }
    private Vector3 GetRandomPosAroundPlayer()
    {
        float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        return transform.position+new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
    }
}
