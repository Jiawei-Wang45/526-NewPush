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
    public float weaponFireRate = 0.2f;    //times per second
    public int weaponBulletAmount;
    public int weaponAmmo;
    public float weaponFiringAngle;
    public float weaponBulletSpread;
    public float reloadTime = 1.0f;

    public string weaponName;
    public GameObject bulletType;  //type of the bullet we are gonna use
    public Sprite weaponImage;

}
