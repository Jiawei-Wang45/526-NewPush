using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShieldGhost : GhostController
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Shield shield;
    protected override void Start()
    {
        base.Start();
    }

    public override void InitializeGhost(Vector2 position, List<ObjectState> playerStates)
    {
        base.InitializeGhost(position, playerStates);
        Instantiate(shield, position, new Quaternion(), transform);
        shield.gameObject.layer = LayerMask.NameToLayer("Shield");
    }
}
