using UnityEngine;
using UnityEngine.TerrainUtils;

public class EnemyController_Tutorial_NoAttack : EnemyController
{
    protected override void Start()
    {
        pc = PlayerController.instance;
        terrainMask = LayerMask.GetMask("Wall", "Player");
    }
    protected override void Update()
    {

    }
    protected override void FixedUpdate()
    {
        Vector2 direction = pc.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        enemyAim.rotation = Quaternion.Slerp(enemyAim.rotation, targetRotation, RotationSpeed * Time.deltaTime/ slowFactor);
    }
}
