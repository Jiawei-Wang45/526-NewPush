using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerForMenu : MonoBehaviour
{
    // UI Components
    [Header("UI Componenets")]
    public GameObject FirstMenu;
    public GameObject CharacterConfig;

    public TextMeshProUGUI weaponText;
    public Image weaponIcon;
    public TextMeshProUGUI attackingAbilityText;
    public Image attackingAbilityIcon;
    public TextMeshProUGUI defenseAbilityText;
    public Image defenseAbilityIcon;

    // CharacterConfig Management
    [Header("CharacterConfig Management")]
    public PlayerBaseWeapon[] weapons;
    public PlayerAbility[] attackingAbilities;
    public PlayerAbility[] defenseAbilities;
    private int weaponIndex=0;
    private int attackingAbilityIndex=0;
    private int defenseAbilityIndex=0;




    // Main Menu button's functions
    private void Awake()
    {
        FirstMenu.SetActive(true);
        CharacterConfig.SetActive(false);
        InitializeConfig();
    }
    public void Tutorial()
    {
        SceneManager.LoadScene("TutorialLevel");
    }
    public void NewGame()
    {
        FirstMenu.SetActive(false);
        CharacterConfig.SetActive(true);
    }
    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit(); 
#endif
    }
    public void Back()
    {
        CharacterConfig.SetActive(false);
        FirstMenu.SetActive(true);
    }
    public void StartGame()
    {
        SaveCharacterConfig();
        SceneManager.LoadScene("AlphaProgressCheck");
    }
    public void InitializeConfig()
    {
        weaponText.text = weapons[weaponIndex].name;
        weaponIcon.sprite = weapons[weaponIndex].weaponIcon;
        attackingAbilityText.text = attackingAbilities[attackingAbilityIndex].abilityName;
        attackingAbilityIcon.sprite = attackingAbilities[attackingAbilityIndex].menuIcon;
        defenseAbilityText.text = defenseAbilities[defenseAbilityIndex].abilityName;
        defenseAbilityIcon.sprite = defenseAbilities[defenseAbilityIndex].menuIcon;
    }
    public void NextWeapon()
    {
        weaponIndex = NextIndexHelper(weaponIndex, weapons.Length);
        weaponText.text = weapons[weaponIndex].name;
        weaponIcon.sprite = weapons[weaponIndex].weaponIcon;
    }
    public void PreviousWeapon()
    {
        weaponIndex = PreviousIndexHelper(weaponIndex,weapons.Length);
        weaponText.text = weapons[weaponIndex].name;
        weaponIcon.sprite = weapons[weaponIndex].weaponIcon;
    }
    public void NextAttackingAbility()
    {
        attackingAbilityIndex = NextIndexHelper(attackingAbilityIndex, attackingAbilities.Length);
        attackingAbilityText.text = attackingAbilities[attackingAbilityIndex].abilityName;
        attackingAbilityIcon.sprite= attackingAbilities[attackingAbilityIndex].menuIcon;
    }
    public void PreviousAttackingAbility()
    {
        attackingAbilityIndex = PreviousIndexHelper(attackingAbilityIndex, attackingAbilities.Length);
        attackingAbilityText.text = attackingAbilities[attackingAbilityIndex].abilityName;
        attackingAbilityIcon.sprite = attackingAbilities[attackingAbilityIndex].menuIcon;
    }
    public void NextDefenseAbility()
    {
        defenseAbilityIndex = NextIndexHelper(defenseAbilityIndex, defenseAbilities.Length);
        defenseAbilityText.text = defenseAbilities[defenseAbilityIndex].abilityName;
        defenseAbilityIcon.sprite = defenseAbilities[defenseAbilityIndex].menuIcon;
    }
    public void PreviousDefenseAbility()
    {
        defenseAbilityIndex = PreviousIndexHelper(defenseAbilityIndex, defenseAbilities.Length);
        defenseAbilityText.text = defenseAbilities[defenseAbilityIndex].abilityName;
        defenseAbilityIcon.sprite = defenseAbilities[defenseAbilityIndex].menuIcon;
    }

    #region helper functions
    private int NextIndexHelper(int currentIndex, int indexRange)
    {
        return (currentIndex + 1) % indexRange;
    }
    private int PreviousIndexHelper(int currentIndex, int indexRange)
    {
        return ((currentIndex - 1) % indexRange + indexRange) % indexRange;
    }
    private void SaveCharacterConfig()
    {
        CharacterConfigHolder.instance.weapon = weapons[weaponIndex];
        CharacterConfigHolder.instance.attackingAbility = attackingAbilities[attackingAbilityIndex];
        CharacterConfigHolder.instance.defenseAbility= defenseAbilities[defenseAbilityIndex];
        CharacterConfigHolder.instance.SetConfigured(true);
    }
    #endregion

}
