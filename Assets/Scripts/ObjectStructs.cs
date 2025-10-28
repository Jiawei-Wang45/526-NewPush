using UnityEngine;
public struct ObjectState
{
    public Vector2 currentVelocity { get; set; }
    public Quaternion currentRotation { get; set; }
    public Vector2 currentPosition { get; set; }
    public ObjectState(Vector2 recordedVelocity, Vector2 recordedPosition, Quaternion recordedRotation)
    {
        currentVelocity = recordedVelocity;
        currentPosition = recordedPosition;
        currentRotation = recordedRotation;
    }
}