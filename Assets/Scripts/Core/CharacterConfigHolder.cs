using UnityEngine;

public class CharacterConfigHolder : MonoBehaviour
{
    static public CharacterConfigHolder instance;
    public PlayerWeapon weapon;
    public PlayerAbility attackingAbility;
    public PlayerAbility defenseAbility;
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
}
