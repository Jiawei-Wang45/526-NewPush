using UnityEngine;

public class PickupWeapon_Tutorial : PickupWeapon
{
    public bool isFirstWeapon;
    public override void Interact()
    {
        AttackTutorialRoom belongedRoom=FindFirstObjectByType<AttackTutorialRoom>();
        if (isFirstWeapon)
        {
            belongedRoom.OnFirstWeaponPicked();
        }
        else
        {
            belongedRoom.OnSecondWeaponPicked();
        }
        base.Interact();
    }
}
