using UnityEngine;

public class WeaponItem_Tutorial : WeaponItem
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
