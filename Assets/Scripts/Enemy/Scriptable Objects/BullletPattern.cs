using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletPattern", menuName = "Enemy Weapons/Bullet Pattern")]
public class BulletPattern : ScriptableObject
{
    public enum bulletDistributionTypes
    {
        Even = 0,
        SemiRandom = 1,
        Random = 2,
        Radial = 3
    }

    public int bulletCount = 1;
    public float firingAngle = 0;
    public bulletDistributionTypes bulletDistribution;

    public int fireCount = 1;

    public float rotateBetweenFiring = 0;
    public float timeBetweenFiring = 0;
}