using UnityEngine;

public class RoomEnemyActivator : MonoBehaviour
{
    public RoomEnemySpawner spawner;

    public void ActivateSpawner()
    {
        if (spawner != null)
        {
            spawner.StartSpawning();
        }
    }

    // ✅ 新增：房间是否所有波次完成
    public bool IsAllWavesCleared()
    {
        return spawner != null && spawner.IsAllWavesCleared;
    }

    // ✅ 保留旧接口：当前波是否清空
    public bool IsRoomCleared()
    {
        return spawner != null && spawner.IsRoomCleared();
    }
}
