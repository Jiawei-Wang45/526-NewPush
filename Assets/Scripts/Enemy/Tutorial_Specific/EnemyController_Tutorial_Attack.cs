using UnityEngine;

public class EnemyController_Tutorial_Attack : EnemyController
{
    protected override void Start()
    {
        pc = PlayerController.instance;
        gameManager = GameManager.instance;

        terrainMask = LayerMask.GetMask("Wall", "Player","OnewayWall");
        RefreshStats();
        if (weapon)
            timeToFire = weapon.fireRate - 0.6f;
        PauseManager.instance.OnPauseStart += PauseStart;
        PauseManager.instance.OnPauseEnd += PauseEnd;
    }
}
