using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "RangedWeapon")]
public class RangedWeapon : PlayerBaseWeapon
{
    [Header("Unique attributes")]
    //bullet attributes
    public float weaponBulletLifeTime;
    public float weaponBulletDamage;
    public float weaponBulletSpeed;


    //weapon attributes
    public float weaponFireInterval;    //times per second
    public int weaponBulletInOneShot;  
    public float weaponFiringAngle;  //scattering angle between bullets, is useless if weaponBulletInOneShot=1
    public float weaponBulletSpread;
    public int maxAmmoNums;
    public float reloadTime = 1.0f;

    public GameObject bulletType;  //type of the bullet we are gonna use
    

}
