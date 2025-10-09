using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyWeapon", menuName = "Enemy Weapons/Weapon")]
public class EnemyWeaponData : ScriptableObject
{
    //bullet attributes
    public float bulletLifeTime;
    public float bulletDamage;
    public float bulletSpeed;

    public GameObject bulletType;
    public float fireRate;
    public BulletPattern bulletPattern;


}