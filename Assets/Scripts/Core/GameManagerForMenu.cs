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
    public GameObject LevelSelection;
    public GameObject CharacterConfig;

    public TextMeshProUGUI weaponText;
    public Image weaponIcon;
    public TextMeshProUGUI attackingAbilityText;
    public Image attackingAbilityIcon;
    public TextMeshProUGUI defenseAbilityText;
    public Image defenseAbilityIcon;

    // Level Selection Management
    [Header("Level Selection")]
    public string[] levelScenes = { "Level_1", "Level_2", "Level_3" };
    public string[] levelNames = { "Level 1", "Level 2", "Level 3" };
    private int selectedLevelIndex = 0;
    
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
        LevelSelection.SetActive(false);
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
        LevelSelection.SetActive(false);
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
        if (LevelSelection.activeSelf)
        {
            // Back from level selection to character config
            LevelSelection.SetActive(false);
            CharacterConfig.SetActive(true);
        }
        else if (CharacterConfig.activeSelf)
        {
            // Back from character config to main menu
            CharacterConfig.SetActive(false);
            FirstMenu.SetActive(true);
        }
    }
    
    // Continue from character config to level selection
    public void ContinueToLevelSelection()
    {
        CharacterConfig.SetActive(false);
        LevelSelection.SetActive(true);
    }
    public void StartGame()
    {
        SaveCharacterConfig();
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelScenes.Length)
        {
            SceneManager.LoadScene(levelScenes[selectedLevelIndex]);
        }
        else
        {
            // Fallback to Level_1 if index is invalid
            SceneManager.LoadScene("Level_1");
        }
    }
    
    // Level Selection Methods
    public void SelectLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            selectedLevelIndex = levelIndex;
            // Save config and start game after selecting level
            StartGame();
        }
    }
    
    public void NextLevel()
    {
        selectedLevelIndex = NextIndexHelper(selectedLevelIndex, levelScenes.Length);
    }
    
    public void PreviousLevel()
    {
        selectedLevelIndex = PreviousIndexHelper(selectedLevelIndex, levelScenes.Length);
    }
    
    public string GetSelectedLevelName()
    {
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelNames.Length)
        {
            return levelNames[selectedLevelIndex];
        }
        return "Unknown Level";
    }
    
    public string GetSelectedLevelScene()
    {
        if (selectedLevelIndex >= 0 && selectedLevelIndex < levelScenes.Length)
        {
            return levelScenes[selectedLevelIndex];
        }
        return "Level_1";
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
        CharacterConfigHolder.instance.weaponList.Clear();
        CharacterConfigHolder.instance.weaponList.Add(weapons[weaponIndex]);
        CharacterConfigHolder.instance.currentWeaponIndex = 0;
        
        CharacterConfigHolder.instance.attackingAbility = attackingAbilities[attackingAbilityIndex];
        CharacterConfigHolder.instance.defenseAbility= defenseAbilities[defenseAbilityIndex];
        CharacterConfigHolder.instance.selectedLevelIndex = selectedLevelIndex;
        CharacterConfigHolder.instance.SetConfigured(true);
    }
    #endregion

}
