using UnityEngine;
public struct ObjectState
{
    public Vector2 currentVelocity { get; set; }
    public Quaternion currentRotation { get; set; }
    public bool currentlyFiring { get; set; }

    public PlayerWeapon usingNewWeapon { get; set; }

    public ObjectState(Vector2 recordedVelocity, Quaternion recordedRotation, bool recordedFiring, PlayerWeapon newWeapon = null)
    {
        currentVelocity = recordedVelocity;
        currentRotation = recordedRotation;
        currentlyFiring = recordedFiring;
        usingNewWeapon = newWeapon;
    }
}