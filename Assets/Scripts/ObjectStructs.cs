using UnityEngine;
public struct ObjectState
{
    public Vector2 currentVelocity { get; set; }
    public Quaternion currentRotation { get; set; }
    public Vector2 currentPosition { get; set; }
    public bool currentlyFiring { get; set; }

    public PlayerWeapon usingNewWeapon { get; set; }
    public string abilityUsed { get; set; }

    public ObjectState(Vector2 recordedVelocity, Vector2 recordedPosition, Quaternion recordedRotation, bool recordedFiring, PlayerWeapon newWeapon = null, string ability = null)
    {
        currentVelocity = recordedVelocity;
        currentPosition = recordedPosition;
        currentRotation = recordedRotation;
        currentlyFiring = recordedFiring;
        usingNewWeapon = newWeapon;
        abilityUsed = ability;
    }
}