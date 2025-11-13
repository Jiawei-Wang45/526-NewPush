using UnityEngine;

public abstract class PlayerBaseWeapon : ScriptableObject
{
    [Header("Base attributes")]   
    public string weaponName;
    public WeaponClass weaponClass;
    public Sprite weaponIcon;
    public Sprite weaponTexture;
}
