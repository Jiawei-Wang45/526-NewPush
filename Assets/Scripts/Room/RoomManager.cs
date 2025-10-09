using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public RoomEnemyActivator enemyActivator;
    public List<DoorController> doors;

    private bool isCleared = false;

    public void OnPlayerEnter()
    {
        if (isCleared) return;
        StartCoroutine(RoomBattleRoutine());
    }

    private IEnumerator RoomBattleRoutine()
    {
        // 1. 锁门
        yield return new WaitForSeconds(0.5f);
        LockDoors();

        // 2. 启动敌人生成
        enemyActivator.ActivateSpawner();

        // 3. 等待所有敌人被清空
        yield return new WaitForSeconds(10f);
        yield return new WaitUntil(() => enemyActivator.IsSpawnerEmpty());

        // 4. 解锁房门
        UnlockDoors();

        isCleared = true;
    }

    private void LockDoors()
    {
        foreach (var door in doors)
            if (door != null) door.Lock();
    }

    private void UnlockDoors()
    {
        foreach (var door in doors)
            if (door != null) door.Unlock();
    }
}
