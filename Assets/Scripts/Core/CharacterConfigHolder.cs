using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterConfigHolder : MonoBehaviour
{
    static public CharacterConfigHolder instance;
    public PlayerBaseWeapon weapon; // Kept for backward compatibility, but use weaponList instead
    public List<PlayerBaseWeapon> weaponList = new List<PlayerBaseWeapon>();
    public int currentWeaponIndex = 0; // Index of currently equipped weapon in weaponList
    public PlayerAbility attackingAbility;
    public PlayerAbility defenseAbility;
    public int selectedLevelIndex = 0;
    [NonSerialized] private bool isConfigured = false;
    private void Awake()
    {
        if (instance != null )
        {
            Destroy(gameObject);
        }
        else
        {
            //Singleton, and also transfer between menu scene and main scene
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void SetConfigured(bool inConfigured)
    {
        isConfigured = inConfigured;
    }
    public bool GetConfigured()
    {
        return isConfigured;
    }
}
