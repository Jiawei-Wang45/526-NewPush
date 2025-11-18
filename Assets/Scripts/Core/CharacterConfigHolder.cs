using System;
using UnityEngine;

public class CharacterConfigHolder : MonoBehaviour
{
    static public CharacterConfigHolder instance;
    public PlayerBaseWeapon weapon;
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
