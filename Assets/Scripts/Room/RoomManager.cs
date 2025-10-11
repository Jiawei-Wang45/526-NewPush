using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        // 锁门
        LockDoors();

        // 开始生成敌人
        enemyActivator.ActivateSpawner();

        // ✅ 等待所有波次结束（而不是第一波）
        yield return new WaitUntil(() => enemyActivator.IsAllWavesCleared());

        // 解锁门
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
