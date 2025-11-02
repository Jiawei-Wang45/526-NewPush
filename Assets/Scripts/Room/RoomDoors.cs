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

    // Try to get the RoomBuilder on this GameObject (or children) to access its prefabs
    private RoomBuilder GetBuilder()
    {
        return GetComponentInChildren<RoomBuilder>(true);
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
        // Close means: for Normal doors activate the door prefab; for PermanentlyLocked
        // replace door with a wall block (or activate the block) so passage is blocked.
        var builder = GetBuilder();
        var all = GetAllDoors();
        for (int i = 0; i < all.Length; i++)
        {
            var dir = (RoomManager.DoorDirection)i;
            var door = all[i];
            var mode = GetDoorMode(dir);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {
                // ensure a wall block exists at this door and hide the door prefab
                if (door != null) door.SetActive(false);
                CreateBlockForDoor(dir, builder);
            }
            else
            {
                // normal door: show door, remove any block
                if (door != null) door.SetActive(true);
                RemoveBlockForDoor(dir);
            }
        }
    }

    public void OpenDoors()
    {
        // Open means: Normal doors are hidden (open), PermanentlyLocked must remain blocked
        var builder = GetBuilder();
        var all = GetAllDoors();
        for (int i = 0; i < all.Length; i++)
        {
            var dir = (RoomManager.DoorDirection)i;
            var door = all[i];
            var mode = GetDoorMode(dir);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {
                // keep blocked
                if (door != null) door.SetActive(false);
                CreateBlockForDoor(dir, builder);
            }
            else
            {
                // open path: hide door and remove block
                if (door != null) door.SetActive(false);
                RemoveBlockForDoor(dir);
            }
        }
    }

    // Create a wall-block GameObject at the door position. If builder or its wallPrefab is missing,
    // no-op.
    private void CreateBlockForDoor(RoomManager.DoorDirection dir, RoomBuilder builder)
    {
        if (builder == null || builder.wallPrefab == null) return;
        string blockName = $"Door_{dir}_Block";
        var existing = transform.Find(blockName);
        if (existing != null) return; // already present

        // Find the door transform to copy its local position/rotation. If door is missing, try to approximate.
        GameObject door = GetDoor(dir);
        GameObject block = Instantiate(builder.wallPrefab, transform);
        block.name = blockName;
        if (door != null)
        {
            block.transform.localPosition = door.transform.localPosition;
            block.transform.localRotation = door.transform.localRotation;
        }
        else
        {
            // approximate by placing at edge using this object's BoxCollider2D if present
            var bc = GetComponent<BoxCollider2D>();
            if (bc != null)
            {
                Vector2 localPos = Vector2.zero;
                float halfX = bc.size.x * 0.5f;
                float halfY = bc.size.y * 0.5f;
                switch (dir)
                {
                    case RoomManager.DoorDirection.North: localPos = new Vector2(0f, halfY); block.transform.localRotation = Quaternion.Euler(0f,0f,90f); break;
                    case RoomManager.DoorDirection.South: localPos = new Vector2(0f, -halfY); block.transform.localRotation = Quaternion.Euler(0f,0f,-90f); break;
                    case RoomManager.DoorDirection.East: localPos = new Vector2(halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
                    case RoomManager.DoorDirection.West: localPos = new Vector2(-halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
                }
                block.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
            }
        }
        // try to ensure block renders above walls if it has SpriteRenderer
        var sr = block.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) sr.sortingOrder += 5;
    }

    private void RemoveBlockForDoor(RoomManager.DoorDirection dir)
    {
        string blockName = $"Door_{dir}_Block";
        var t = transform.Find(blockName);
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
