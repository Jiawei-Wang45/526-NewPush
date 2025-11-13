using UnityEditor.Animations;
using UnityEngine;
[CreateAssetMenu(fileName = "New Weapon", menuName = "MeleeWeapon")]
public class MeleeWeapon : PlayerBaseWeapon
{
    [Header("Unique attributes")]
    public float damage;
    public float slashCD;
    public float reflectCD;
    public WieldEffect slashEffectPrefab;
    public WieldEffect reflectEffectPrefab;
    public RuntimeAnimatorController animatorController;

}
