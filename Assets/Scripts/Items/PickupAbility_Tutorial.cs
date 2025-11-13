using UnityEngine;

public class PickupAbility_Tutorial : PickupAbility
{
    public bool isFirstAbility;

    public override void Interact()
    {
        AbilityTutorialRoom belongedRoom = FindFirstObjectByType<AbilityTutorialRoom>();
        if (isFirstAbility)
        {
            belongedRoom.OnClusterBombPicked();
        }
        else
        {
            belongedRoom.OnInvincibleDashPicked();
        }
        base.Interact();
    }
}
