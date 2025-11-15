using UnityEngine;

public class AbilityItem_Tutorial : AbilityItem
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
