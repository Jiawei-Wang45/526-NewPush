using System;
using UnityEngine;

public class MeleeWeaponInstance : MonoBehaviour
{
    [NonSerialized] private PlayerAim playerAim;
    public void InitPlayerAim(PlayerAim inPlayerAim)
    {
        playerAim = inPlayerAim;
    }
    public void StartAttacking()
    {
        playerAim.SetAttacking(true);
    }
    public void EndAttacking()
    {
        playerAim.SetAttacking(false);
    }
    private void OnDestroy()
    {
        EndAttacking();
    }
}
