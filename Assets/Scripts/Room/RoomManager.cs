using System.Collections;
using UnityEngine;

// Controls a single room: when the player enters, close doors and start spawning enemies;
// when all enemies inside the room are dead, open doors again.
public class RoomManager : MonoBehaviour
{
    [Header("Room configuration")]
    // Trigger collider that defines the room area. Should be set as a Trigger and cover the room.
    public Collider2D roomTrigger;

    // Doors to activate when room is closed. Each side can be assigned or left null.
    public enum DoorDirection { North, East, South, West }

    [Header("Door prefab (optional)")]
    // Assign a door prefab to have the RoomManager instantiate doors for each side at runtime.
    // If left null, the manager will create simple placeholder GameObjects for doors as before.
    public GameObject doorPrefab;

    // Instantiated door instances (created from doorPrefab at runtime if provided).
    private GameObject doorNorth;
    private GameObject doorEast;
    private GameObject doorSouth;
    private GameObject doorWest;

    [Header("Runtime door settings")]
    // How far (world units) the door should be placed outside the room trigger bounds.
    // This was previously hard-coded as 1f in EnsureDoorsExist. Exposed so generators can read it.
    public float doorOutsideOffset = 1f;

    // Return a world-space point representing the logical exit for the given door direction.
    // If the door GameObject contains a child named "ExitPoint" that Transform will be used
    // (allowing prefab authors to customize the exact anchor). Otherwise the door GameObject
    // position is returned.
    public Vector3 GetDoorEndpoint(DoorDirection dir)
    {
        var door = GetDoor(dir);
        if (door == null) return Vector3.zero;
        var exit = door.transform.Find("ExitPoint");
        if (exit != null) return exit.position;
        return door.transform.position;
    }

    // Helper accessors for external systems (e.g. procedural generator) to query existing doors.
    public GameObject GetDoor(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North: return doorNorth;
            case DoorDirection.East: return doorEast;
            case DoorDirection.South: return doorSouth;
            case DoorDirection.West: return doorWest;
            default: return null;
        }
    }

    public bool HasDoor(DoorDirection dir) => GetDoor(dir) != null;

    public GameObject[] GetAllDoors()
    {
        return new GameObject[] { doorNorth, doorEast, doorSouth, doorWest };
    }

    public enum DoorMode { Normal, PermanentlyLocked }
    [Header("Door modes")]
    public DoorMode doorNorthMode = DoorMode.Normal;
    public DoorMode doorEastMode = DoorMode.Normal;
    public DoorMode doorSouthMode = DoorMode.Normal;
    public DoorMode doorWestMode = DoorMode.Normal;

    public DoorMode GetDoorMode(DoorDirection dir)
    {
        switch (dir)
        {
            case DoorDirection.North: return doorNorthMode;
            case DoorDirection.East: return doorEastMode;
            case DoorDirection.South: return doorSouthMode;
            case DoorDirection.West: return doorWestMode;
            default: return DoorMode.Normal;
        }
    }

    public void SetDoorMode(DoorDirection dir, DoorMode mode)
    {
        switch (dir)
        {
            case DoorDirection.North: doorNorthMode = mode; break;
            case DoorDirection.East: doorEastMode = mode; break;
            case DoorDirection.South: doorSouthMode = mode; break;
            case DoorDirection.West: doorWestMode = mode; break;
        }
    }

    // Enemy spawners inside this room. These will be started when the player enters.
    public EnemySpawner[] enemySpawners;

    [Header("Runtime")]
    // Set to true while the room is active (player in room and enemies spawning/ alive)
    public bool isRoomActive = false;

    // Delegate invoked when the room has been cleared
    public delegate void OnRoomClearedDelegate();
    public OnRoomClearedDelegate onRoomCleared;

    // Guard to ensure we only trigger entry once per activation
    private bool hasPlayerEntered = false;

    private void Reset()
    {
        // Try to auto-assign the trigger if this script is attached to the same object that has the collider
        if (roomTrigger == null)
            roomTrigger = GetComponent<Collider2D>();

        // If still null, try to find a child Collider2D (common prefab layout)
        if (roomTrigger == null)
            roomTrigger = GetComponentInChildren<Collider2D>();
    }

    private void Awake()
    {
        // Basic validation
        if (roomTrigger == null)
            Debug.LogWarning($"RoomManager '{name}': roomTrigger is not assigned. Assign a Trigger Collider that defines the room bounds.");
    }

    private void Start()
    {
        // Ensure door objects exist and are positioned before opening
        EnsureDoorsExist();
        // Ensure doors start open so player can enter
        OpenDoors();
        isRoomActive = false;
        hasPlayerEntered = false;
    }

    // Ensure door GameObjects exist (create placeholders if missing) and try to place them at room edges
    // Made public so external systems (e.g. ProceduralDungeonGenerator) can force creation/placement
    public void EnsureDoorsExist()
    {
        // helper to create or instantiate a door
        GameObject CreateDoorIfNull(ref GameObject door, string name)
        {
            if (door == null)
            {
                if (doorPrefab != null)
                {
                    // Instantiate from prefab as child of this room
                    door = Instantiate(doorPrefab, this.transform);
                    door.name = name;
                }
                else
                {
                    // Fallback: create a placeholder empty GameObject
                    door = new GameObject(name);
                    door.transform.SetParent(this.transform, false);
                }
            }
            return door;
        }

        CreateDoorIfNull(ref doorNorth, "Door_North");
        CreateDoorIfNull(ref doorEast, "Door_East");
        CreateDoorIfNull(ref doorSouth, "Door_South");
        CreateDoorIfNull(ref doorWest, "Door_West");

    // Try to position doors at the edges of the roomTrigger if available
    if (roomTrigger != null)
        {
            // use world-space bounds to compute edge positions
            var b = roomTrigger.bounds;
            // place doors outside the collider bounds by configured offset
            float outsideOffset = doorOutsideOffset;

            if (doorNorth != null)
            {
                doorNorth.transform.position = b.center + new Vector3(0f, b.extents.y + outsideOffset, 0f);
                // rotate north door so it faces the correct direction (north); keep east/west as default
                doorNorth.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

            if (doorSouth != null)
            {
                doorSouth.transform.position = b.center + new Vector3(0f, -b.extents.y - outsideOffset, 0f);
                // rotate south door to face south
                doorSouth.transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            }

            if (doorEast != null)
            {
                doorEast.transform.position = b.center + new Vector3(b.extents.x + outsideOffset, 0f, 0f);
                // east door uses prefab's default rotation
            }

            if (doorWest != null)
            {
                doorWest.transform.position = b.center + new Vector3(-b.extents.x - outsideOffset, 0f, 0f);
                // west door uses prefab's default rotation
            }
        }
    }

    // Public helper to make sure doors are created and the open/closed visual state is applied.
    // This is useful when rooms are instantiated by editor-time generators which call RoomManager
    // methods immediately after Instantiate (Start may not have executed yet in that context).
    public void InitializeDoors()
    {
        EnsureDoorsExist();
        // Apply open/closed logic using existing private method
        OpenDoors();
        // Ensure spawners have a reference to the GameManager when rooms are created by generators
        AssignGameManagerToSpawners();
    }

    // Ensure any EnemySpawner on this room has a reference to the global GameManager instance.
    // This is defensive: generators or designers might forget to wire the reference in the Inspector.
    private void AssignGameManagerToSpawners()
    {
        if (enemySpawners == null || GameManager.instance == null) return;
        foreach (var s in enemySpawners)
        {
            if (s == null) continue;
            if (s.gameManager == null)
            {
                s.gameManager = GameManager.instance;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (roomTrigger != null && other == null) return;

        // Only respond to player entering. Avoid starting the room for other colliders.
        var player = other.GetComponentInParent<PlayerControllerTest>();
        if (player != null && !hasPlayerEntered)
        {
            // Prefer to verify the player's collider is fully inside the room trigger
            bool fullyInside = false;
            var playerCol = player.GetComponentInChildren<Collider2D>();
            if (playerCol != null && roomTrigger != null)
            {
                var pBounds = playerCol.bounds;
                fullyInside = roomTrigger.bounds.Contains(pBounds.min) && roomTrigger.bounds.Contains(pBounds.max);
            }
            else if (roomTrigger != null)
            {
                // fallback: use player position
                fullyInside = roomTrigger.bounds.Contains(player.transform.position);
            }

            if (fullyInside)
            {
                hasPlayerEntered = true;
                StartRoom();
            }
            else
            {
                // Player is only partially inside; wait for OnTriggerStay2D to detect full entry
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (roomTrigger != null && other == null) return;
        var player = other.GetComponentInParent<PlayerControllerTest>();
        if (player != null && !hasPlayerEntered)
        {
            bool fullyInside = false;
            var playerCol = player.GetComponentInChildren<Collider2D>();
            if (playerCol != null && roomTrigger != null)
            {
                var pBounds = playerCol.bounds;
                fullyInside = roomTrigger.bounds.Contains(pBounds.min) && roomTrigger.bounds.Contains(pBounds.max);
            }
            else if (roomTrigger != null)
            {
                fullyInside = roomTrigger.bounds.Contains(player.transform.position);
            }

            if (fullyInside)
            {
                hasPlayerEntered = true;
                StartRoom();
            }
        }
    }

    // Made public so DungeonManager can activate rooms
    public void StartRoom()
    {
        isRoomActive = true;
        CloseDoors();
        StartSpawners();

        // Determine how many spawners are finite (will eventually finish).
        int spawnersToFinish = 0;
        if (enemySpawners != null && enemySpawners.Length > 0)
        {
            foreach (var s in enemySpawners)
            {
                if (s == null) continue;
                // subscribe to finished event so we know when finite spawners complete
                s.OnSpawnerFinished += OnSpawnerFinished;
                if (s.HasFiniteWaves()) spawnersToFinish++;
            }
        }

        if (spawnersToFinish > 0)
        {
            // We'll wait for the OnSpawnerFinished callbacks to open doors.
            pendingSpawnersToFinish = spawnersToFinish;
        }
        else
        {
            // No finite spawners; poll spawners until they are all idle (no active waves)
            StartCoroutine(PollSpawnersUntilIdle());
        }
    }

    private int pendingSpawnersToFinish = 0;

    private void OnSpawnerFinished()
    {
        pendingSpawnersToFinish--;
        if (pendingSpawnersToFinish <= 0)
        {
            // Room cleared
            isRoomActive = false;
            OpenDoors();
            onRoomCleared?.Invoke();
            // Notify game manager that this room is cleared
            GameManager.instance?.RoomCleared(this);
        }
    }

    private IEnumerator PollSpawnersUntilIdle()
    {
        // Wait a frame so spawners can initialize
        yield return null;

        while (true)
        {
            bool anyActive = false;
            if (enemySpawners != null)
            {
                foreach (var s in enemySpawners)
                {
                    if (s == null) continue;
                    if (s.IsWaveActive)
                    {
                        anyActive = true;
                        break;
                    }
                }
            }

            if (!anyActive)
            {
                Debug.Log($"RoomManager '{name}': All spawners idle, room cleared.");
                // No active waves across spawners — consider room cleared
                isRoomActive = false;
                OpenDoors();
                onRoomCleared?.Invoke();
                // Notify game manager that this room is cleared
                GameManager.instance?.RoomCleared(this);
                yield break;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    private void CloseDoors()
    {
        var all = GetAllDoors();
        if (all == null) return;
        foreach (var d in all)
        {
            if (d == null) continue;
            d.SetActive(true);
        }
    }

    private void OpenDoors()
    {
        var all = GetAllDoors();
        if (all == null) return;
        // Only open doors that are not permanently locked
        for (int i = 0; i < all.Length; i++)
        {
            var d = all[i];
            if (d == null) continue;
            var mode = GetDoorMode((DoorDirection)i);
            if (mode == DoorMode.PermanentlyLocked)
            {
                d.SetActive(true); // keep closed
            }
            else
            {
                d.SetActive(false);
            }
        }
    }

    private void StartSpawners()
    {
        if (enemySpawners == null) return;
        foreach (var s in enemySpawners)
        {
            if (s == null) continue;
            // Try to call StartWave if available (matches GameManager usage)
            s.StartWave();
        }
    }

    // Optional helper to force-clear the room (useful for debugging / level design)
    public void ForceClearRoom()
    {
        StopAllCoroutines();
        isRoomActive = false;
        OpenDoors();
        onRoomCleared?.Invoke();
    }
}
