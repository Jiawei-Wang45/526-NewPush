using UnityEngine;

[CreateAssetMenu(fileName = "EnemyMovementPattern", menuName = "Enemy Movements/Movement Pattern")]
public class EnemyMovementPattern : ScriptableObject
{
    public enum idleBehaviors
    {
        Stops = 0,
        RandomWalk = 1
    }

    public idleBehaviors idleBehavior;

    //public float comfortableDistance = 0.0f;
    public float TargetStoppingDistance = 6.0f;
    public float MovementSpeed = 1.0f;
    public float LeashTime = -1.0f;

    //public float BackoffSpeedFactor = 1.0f;
}
