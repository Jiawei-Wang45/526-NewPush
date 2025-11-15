using UnityEngine;
using UnityEngine.InputSystem;

public class ClusterBombAbility : BaseAbility
{
    public Bullet_Cluster clusterBullet;
    public float shootingCooldown=5.0f;
    private Transform attackPoint;
    protected override void Awake()
    {
        base.Awake();
        abilityType = AbilityType.Attacking;
        attackPoint = pc.transform.Find("PlayerAim/AttackPoint");
    }
    //private void Start()
    //{
    //    pc.playerInput.Default.AttackingAbility.performed += OnClusterBombTriggered;
    //    //GameManager.instance.onReset += ResetStates;
    //}
    public override void ActivateAbility() 
    {
        ActivateClusterBomb();
    }
    private void ActivateClusterBomb()
    {
        if (isCooldown) return;
        StartCoroutine(AbilityCooldownCoroutine(shootingCooldown));
        Bullet_Cluster spawnedBullet = Instantiate(clusterBullet, attackPoint.position, attackPoint.rotation);
        spawnedBullet.InitBullet(spawnedBullet.parentBulletSpeed, spawnedBullet.parentBulletDamage);
        SendAnalytics("ClusterBomb");
    }
    //protected override void ResetStates()
    //{
    //   if (isCooldown)
    //    {
    //        base.ResetStates();
    //    }
    //}
    //private void OnDestroy()
    //{
    //    pc.playerInput.Default.AttackingAbility.performed -= OnClusterBombTriggered;
    //    //GameManager.instance.onReset -= ResetStates;
    //}
}
