using UnityEngine;

// Extracted door management for a room: tracking door GameObjects, modes and open/close logic.
// This component is intended to be attached to the same GameObject as RoomManager.
public class RoomDoors : MonoBehaviour
{
    // Door instances are created by RoomBuilder and discovered at runtime; do not assign manually
    public GameObject doorNorth;
    public GameObject doorEast;
    public GameObject doorSouth;
    public GameObject doorWest;

    // Door mode fields mirror RoomManager's DoorMode; kept here so designers can edit per-room
    public RoomManager.DoorMode doorNorthMode = RoomManager.DoorMode.Normal;
    public RoomManager.DoorMode doorEastMode = RoomManager.DoorMode.Normal;
    public RoomManager.DoorMode doorSouthMode = RoomManager.DoorMode.Normal;
    public RoomManager.DoorMode doorWestMode = RoomManager.DoorMode.Normal;

    // Discover child door GameObjects by conventional names if present
    public void EnsureDoorsExist()
    {
        var foundN = transform.Find("Door_North"); if (foundN != null) doorNorth = foundN.gameObject;
        var foundE = transform.Find("Door_East"); if (foundE != null) doorEast = foundE.gameObject;
        var foundS = transform.Find("Door_South"); if (foundS != null) doorSouth = foundS.gameObject;
        var foundW = transform.Find("Door_West"); if (foundW != null) doorWest = foundW.gameObject;
    }

    public GameObject GetDoor(RoomManager.DoorDirection dir)
    {
        switch (dir)
        {
            case RoomManager.DoorDirection.North: return doorNorth;
            case RoomManager.DoorDirection.East: return doorEast;
            case RoomManager.DoorDirection.South: return doorSouth;
            case RoomManager.DoorDirection.West: return doorWest;
            default: return null;
        }
    }

    public bool HasDoor(RoomManager.DoorDirection dir) => GetDoor(dir) != null;

    public GameObject[] GetAllDoors() => new GameObject[] { doorNorth, doorEast, doorSouth, doorWest };

    public RoomManager.DoorMode GetDoorMode(RoomManager.DoorDirection dir)
    {
        switch (dir)
        {
            case RoomManager.DoorDirection.North: return doorNorthMode;
            case RoomManager.DoorDirection.East: return doorEastMode;
            case RoomManager.DoorDirection.South: return doorSouthMode;
            case RoomManager.DoorDirection.West: return doorWestMode;
            default: return RoomManager.DoorMode.Normal;
        }
    }

    public void SetDoorMode(RoomManager.DoorDirection dir, RoomManager.DoorMode mode)
    {
        switch (dir)
        {
            case RoomManager.DoorDirection.North: doorNorthMode = mode; break;
            case RoomManager.DoorDirection.East: doorEastMode = mode; break;
            case RoomManager.DoorDirection.South: doorSouthMode = mode; break;
            case RoomManager.DoorDirection.West: doorWestMode = mode; break;
        }
    }

    public void CloseDoors()
    {
        var all = GetAllDoors();
        if (all == null) return;
        foreach (var d in all)
        {
            if (d == null) continue;
            d.SetActive(true);
        }
    }

    public void OpenDoors()
    {
        var all = GetAllDoors();
        if (all == null) return;
        for (int i = 0; i < all.Length; i++)
        {
            var d = all[i];
            if (d == null) continue;
            var mode = GetDoorMode((RoomManager.DoorDirection)i);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {
                d.SetActive(true);
            }
            else
            {
                d.SetActive(false);
            }
        }
    }
}
