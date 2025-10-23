using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class PlayerWeapon : ScriptableObject
{
    public WeaponType weaponType;
    public TriggerType triggerType;

    //bullet attributes
    public float weaponBulletLifeTime;
    public float weaponBulletDamage;
    public float weaponBulletSpeed;


    //weapon attributes
    public float weaponFireRate;    //times per second
    public int weaponBulletInOneShot;  
    public float weaponFiringAngle;  //scattering angle between bullets, is useless if weaponBulletInOneShot=1
    public float weaponBulletSpread;
    public int maxAmmoNums;

    public string weaponName;
    public GameObject bulletType;  //type of the bullet we are gonna use
    public Sprite weaponImage;

}
