using UnityEngine;

[CreateAssetMenu(fileName = "New Ability", menuName = "Ability")]
public class PlayerAbility : ScriptableObject
{
    public AbilityClass abilityClass;
    public string abilityName;
    public Sprite menuIcon;
    public Sprite cooldownIcon;
    public GameObject abilityPrefab;
}
