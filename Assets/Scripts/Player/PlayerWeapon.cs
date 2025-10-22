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
    public float weaponFireRate = 0.2f;

    //weapon attributes
    public float weaponFireSpeed;    //times per second
    public int weaponBulletInOneShot;  
    public float weaponFiringAngle;
    public float weaponBulletSpread;
    public int maxAmmoNums;

    public string weaponName;
    public GameObject bulletType;  //type of the bullet we are gonna use
    public Sprite weaponImage;

}
