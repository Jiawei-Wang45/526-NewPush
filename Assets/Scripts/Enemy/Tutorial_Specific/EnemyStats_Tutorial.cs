using System;
using UnityEngine;

public class EnemyStats_Tutorial : EnemyStats
{

    public enum SceneType
    {
        AttackRoomFirst,
        AttackRoomSecond,
        DefenseRoom,
        AbilityRoom,
        TestRoom
    }
    [NonSerialized] public SceneType type;
    [NonSerialized] public int index;
    public override void TakeDamage(float damage)
    {
        SetHealth(health - damage);
        if (health <= 0)
        {
            RandomDropItems();
            GameObject particle = Instantiate(dyingEffect, transform.position, new Quaternion());
            switch(type)
            {
                case SceneType.AttackRoomFirst:
                    FindFirstObjectByType<AttackTutorialRoom>().OnFirstEnemyKilled();
                    break;
                case SceneType.AttackRoomSecond:
                    FindFirstObjectByType<AttackTutorialRoom>().OnSecondEnemyKilled();
                    break;
                case SceneType.DefenseRoom:
                    FindFirstObjectByType<DefenseTutorialRoom>().EndBounce();
                    break;
                case SceneType.AbilityRoom:
                    FindFirstObjectByType<AbilityTutorialRoom>().OnEnemyDestroyed(index);
                    break;
                case SceneType.TestRoom:
                    FindFirstObjectByType<TestTutorialRoom>().OnEnemyDestroyed(index);
                    break;

            }
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(FlashCoroutine());
        }
    }

}
