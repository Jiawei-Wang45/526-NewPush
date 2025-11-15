using UnityEngine;

// Static helper for door management. Methods operate on a RoomManager instance passed as context.
public static class RoomDoors
{
    // Discover child door GameObjects by conventional names if present and assign into RoomManager
    public static void EnsureDoorsExist(RoomManager rm)
    {
        if (rm == null) return;
        // Use recursive search to be robust in builds where hierarchy or naming may differ.
        var transforms = rm.GetComponentsInChildren<Transform>(true);
        foreach (var tt in transforms)
        {
            var n = tt.name;
            if (n.StartsWith("Door_North")) rm.doorNorth = tt.gameObject;
            else if (n.StartsWith("Door_East")) rm.doorEast = tt.gameObject;
            else if (n.StartsWith("Door_South")) rm.doorSouth = tt.gameObject;
            else if (n.StartsWith("Door_West")) rm.doorWest = tt.gameObject;
        }
    }

    public static GameObject GetDoor(RoomManager rm, RoomManager.DoorDirection dir)
    {
        if (rm == null) return null;
        switch (dir)
        {
            case RoomManager.DoorDirection.North: return rm.doorNorth;
            case RoomManager.DoorDirection.East: return rm.doorEast;
            case RoomManager.DoorDirection.South: return rm.doorSouth;
            case RoomManager.DoorDirection.West: return rm.doorWest;
            default: return null;
        }
    }

    public static bool HasDoor(RoomManager rm, RoomManager.DoorDirection dir) => GetDoor(rm, dir) != null;

    public static GameObject[] GetAllDoors(RoomManager rm) => new GameObject[] { rm.doorNorth, rm.doorEast, rm.doorSouth, rm.doorWest };

    public static RoomManager.DoorMode GetDoorMode(RoomManager rm, RoomManager.DoorDirection dir)
    {
        if (rm == null) return RoomManager.DoorMode.Normal;
        switch (dir)
        {
            case RoomManager.DoorDirection.North: return rm.doorNorthMode;
            case RoomManager.DoorDirection.East: return rm.doorEastMode;
            case RoomManager.DoorDirection.South: return rm.doorSouthMode;
            case RoomManager.DoorDirection.West: return rm.doorWestMode;
            default: return RoomManager.DoorMode.Normal;
        }
    }

    public static void SetDoorMode(RoomManager rm, RoomManager.DoorDirection dir, RoomManager.DoorMode mode)
    {
        if (rm == null) return;
        switch (dir)
        {
            case RoomManager.DoorDirection.North: rm.doorNorthMode = mode; break;
            case RoomManager.DoorDirection.East: rm.doorEastMode = mode; break;
            case RoomManager.DoorDirection.South: rm.doorSouthMode = mode; break;
            case RoomManager.DoorDirection.West: rm.doorWestMode = mode; break;
        }
    }

    public static void CloseDoors(RoomManager rm)
    {
        if (rm == null) return;
        var all = GetAllDoors(rm);
        for (int i = 0; i < all.Length; i++)
        {
            var dir = (RoomManager.DoorDirection)i;
            var door = all[i];
            var mode = GetDoorMode(rm, dir);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {
                if (door != null) door.SetActive(false);
                CreateBlockForDoor(rm, dir);
            }
            else
            {
                if (door != null) door.SetActive(true);
                RemoveBlockForDoor(rm, dir);
            }
        }
    }

    public static void OpenDoors(RoomManager rm)
    {
        if (rm == null) return;
        var all = GetAllDoors(rm);
        for (int i = 0; i < all.Length; i++)
        {
            var dir = (RoomManager.DoorDirection)i;
            var door = all[i];
            var mode = GetDoorMode(rm, dir);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {
                if (door != null) door.SetActive(false);
                CreateBlockForDoor(rm, dir);
            }
            else
            {
                if (door != null) door.SetActive(false);
                RemoveBlockForDoor(rm, dir);
            }
        }
    }

    // Create a wall-block GameObject at the door position. Uses RoomManager's wallPrefab if available.
    private static void CreateBlockForDoor(RoomManager rm, RoomManager.DoorDirection dir)
    {
        if (rm == null || rm.wallPrefab == null) return;
        string blockName = $"Door_{dir}_Block";
        var parent = rm.transform;
        var existing = parent.Find(blockName);
        if (existing != null) return; // already present

        GameObject door = GetDoor(rm, dir);
        GameObject block = Object.Instantiate(rm.wallPrefab, parent);
        block.name = blockName;
        if (door != null)
        {
            block.transform.localPosition = door.transform.localPosition;
            block.transform.localRotation = door.transform.localRotation;
        }
        // for safety purpose 
        else
        {
            var bc = rm.gameObject.GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                Vector2 localPos = Vector2.zero;
                float halfX = bc.size.x * 0.5f;
                float halfY = bc.size.y * 0.5f;
                switch (dir)
                {
                    case RoomManager.DoorDirection.North: localPos = new Vector2(0f, halfY); block.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); break;
                    case RoomManager.DoorDirection.South: localPos = new Vector2(0f, -halfY); block.transform.localRotation = Quaternion.Euler(0f, 0f, -90f); break;
                    case RoomManager.DoorDirection.East: localPos = new Vector2(halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
                    case RoomManager.DoorDirection.West: localPos = new Vector2(-halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
                }
                block.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            }
        }
        var sr = block.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder += 5;
    }

    private static void RemoveBlockForDoor(RoomManager rm, RoomManager.DoorDirection dir)
    {
        if (rm == null) return;
        string blockName = $"Door_{dir}_Block";
        var t = rm.transform.Find(blockName);
        if (t != null)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(t.gameObject);
#else
            Object.Destroy(t.gameObject);
#endif
        }
    }
}
