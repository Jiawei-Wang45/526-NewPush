using UnityEngine;

public class RoomEnemyActivator : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    private bool activated = false;

    private void Awake()
    {
        if (enemySpawner != null)
            enemySpawner.enabled = false; // 禁用，不让 Start() 执行
    }

    public void ActivateSpawner()
    {
        if (activated) return;

        activated = true;
        if (enemySpawner != null)
            enemySpawner.enabled = true; // 启用，Start() 自动运行
    }

    // 检查当前房间是否还有敌人（辅助 RoomManager）
    public bool IsSpawnerEmpty()
    {
        // 若 enemySpawner 不在场景中则认为清空
        if (enemySpawner == null) return true;

        // 检查场景中是否还有敌人（根据 Layer）
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        foreach (GameObject obj in FindObjectsOfType<GameObject>())
        {
            if (obj.layer == enemyLayer)
                return false;
        }
        return true;
    }
}
