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
    public float weaponFireSpeed;    //times per second
    public int weaponBulletAmount;
    public float weaponFiringAngle;
    public float weaponBulletSpread;

    //ammo attributes
    public int maxAmmo = 100;
    public int ammoPerShot = 1;
    public bool hasInfiniteAmmo = false;

    public string weaponName;
    public GameObject bulletType;  //type of the bullet we are gonna use
    public Sprite weaponImage;

}
