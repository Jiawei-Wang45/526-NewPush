using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

public static class ProceduralDungeonInstantiator
{
    // Instantiate rooms and roads from a GenerationResult produced by ProceduralGraphGenerator
    public static List<GameObject> InstantiateFromGraph(
        ProceduralGraphGenerator.GenerationResult graphResult,
        GameObject[] roomPrefabs,
        GameObject startRoomPrefab,
        GameObject endRoomPrefab,
        GameObject roadPrefab,
        Vector2 cellSize,
        float roomScale,
        Vector3 dungeonOffset,
        Transform parent)
    {
        var instantiatedRooms = new List<GameObject>();
        var occupiedCells = graphResult.occupiedCells;
        var adjacencyGraph = graphResult.adjacencyGraph;

        // Build cell->index map so we can query neighbors quickly
        var cellToIndex = new Dictionary<Vector2Int, int>(occupiedCells.Count);
        for (int ci = 0; ci < occupiedCells.Count; ci++) cellToIndex[occupiedCells[ci]] = ci;

        // Instantiate rooms at cell centers and initialize door modes based on adjacency
        Vector2Int[] cardinalDirs = new Vector2Int[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        for (int i = 0; i < occupiedCells.Count; i++)
        {
            var cell = occupiedCells[i];
            Vector3 worldPos = new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f) + dungeonOffset;
            //Vector3 worldPos = new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f) + parent.position + dungeonOffset;
            GameObject roomInstance = null;
            GameObject prefabToUse = null;
            if (i == graphResult.startIndex && startRoomPrefab != null) prefabToUse = startRoomPrefab;
            else if (i == graphResult.endIndex && endRoomPrefab != null) prefabToUse = endRoomPrefab;
            else if (roomPrefabs != null && roomPrefabs.Length > 0) prefabToUse = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];

            if (prefabToUse != null)
            {
                roomInstance = Object.Instantiate(prefabToUse, worldPos, Quaternion.identity, parent);
            }
            else
            {
                roomInstance = new GameObject($"Room_{i}");
                roomInstance.transform.position = worldPos;
                roomInstance.transform.SetParent(parent, false);
            }

            // Ensure a RoomManager is present and configured
            var rm = roomInstance.GetComponent<RoomManager>();
            if (rm == null) rm = roomInstance.AddComponent<RoomManager>();

            // Apply scale so any built geometry matches intended world size
            roomInstance.transform.localScale = Vector3.one * roomScale;

            // Determine connectivity for each cardinal direction from adjacency graph
            for (int d = 0; d < cardinalDirs.Length; d++)
            {
                var nbCell = cell + cardinalDirs[d];
                bool connected = false;
                if (cellToIndex.TryGetValue(nbCell, out int nbIndex))
                {                    
                    if (adjacencyGraph[i].Contains(nbIndex)) connected = true;
                }

                var dirEnum = (RoomManager.ObstacleDirection)d;
                if (!connected)
                    rm.SetDoorMode(dirEnum, RoomManager.DoorMode.PermanentlyLocked);
            }

            // Use RoomManager's settings and static RoomBuilder to create or update geometry
            RoomBuilder.Build(roomInstance.transform, rm.defaultSize, rm.floorPrefab, rm.wallPrefab, rm.doorPrefab, rm.wallThickness, rm.doorOutsideOffset, rm.clearExistingChildren);
            //rm.roomTrigger = roomInstance.GetComponentInChildren<BoxCollider2D>(true);
            // Ensure doors exist and apply open/closed state immediately

            //rm.InitializeDoors();

            instantiatedRooms.Add(roomInstance);
        }

        // Create roads
        if (roadPrefab != null)
        {
            //var created = new HashSet<string>();
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                foreach (var j in adjacencyGraph[i])
                {
                    //if (i == j) continue;
                    if (j < i) continue;
                    //string key = i < j ? $"{i}-{j}" : $"{j}-{i}";
                    //if (created.Contains(key)) continue;
                    //created.Add(key);
                    CreateRoadBetween(instantiatedRooms, occupiedCells, i, j, roadPrefab, cellSize, parent);
                }
            }
        }

        return instantiatedRooms;
    }

    private static void CreateRoadBetween(List<GameObject> instantiatedRooms, List<Vector2Int> occupiedCells, int indexA, int indexB, GameObject roadPrefab, Vector2 cellSize, Transform parent)
    {
        var aCell = occupiedCells[indexA];
        var bCell = occupiedCells[indexB];
        Vector3 aPos;
        Vector3 bPos;
        GetEndpointForConnection(instantiatedRooms, occupiedCells, indexA, indexB,out aPos,out bPos);

        TileRoadBetween(aPos, bPos, roadPrefab, cellSize, parent);
    }

    private static void GetEndpointForConnection(List<GameObject> instantiatedRooms, List<Vector2Int> occupiedCells, int indexA, int indexB, out Vector3 aPos,out Vector3 bPos)
    {
        var roomA = instantiatedRooms[indexA];
        var roomB=  instantiatedRooms[indexB];
        Vector2Int dir = occupiedCells[indexB] - occupiedCells[indexA];
        RoomManager.ObstacleDirection forwardDir;
        RoomManager.ObstacleDirection reverseDir;

        if (dir.x > 0)
        {
            forwardDir = RoomManager.ObstacleDirection.East;
            reverseDir= RoomManager.ObstacleDirection.West;
        }
        else if (dir.x < 0)
        {
            forwardDir = RoomManager.ObstacleDirection.West;
            reverseDir= RoomManager.ObstacleDirection.East;
        }
        else if (dir.y > 0)
        {
            forwardDir = RoomManager.ObstacleDirection.North;
            reverseDir= RoomManager.ObstacleDirection.South;
        }
        else
        {
            forwardDir = RoomManager.ObstacleDirection.South;
            reverseDir= RoomManager.ObstacleDirection.North;
        }
        aPos = roomA.GetComponent<RoomManager>().GetDoorEndpoint(forwardDir);
        bPos=roomB.GetComponent<RoomManager>().GetDoorEndpoint(reverseDir);
        //Vector3 center = roomGO != null ? roomGO.transform.position : new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f);
        //Vector3 offset = Vector3.zero;
        //if (dir.x > 0) offset = new Vector3(cellSize.x * 0.5f, 0f, 0f);
        //else if (dir.x < 0) offset = new Vector3(-cellSize.x * 0.5f, 0f, 0f);
        //else if (dir.y > 0) offset = new Vector3(0f, cellSize.y * 0.5f, 0f);
        //else if (dir.y < 0) offset = new Vector3(0f, -cellSize.y * 0.5f, 0f);

        //return center + offset;

    }

    private static void TileRoadBetween(Vector3 from, Vector3 to, GameObject roadPrefab, Vector2 cellSize, Transform parent)
    {
        Vector3 dir = to - from;
        float totalLength = dir.magnitude;
        if (totalLength <= 0.001f) return;
        Vector3 unit = dir.normalized;

        float segLen = MeasureRoadPrefabLength(roadPrefab);
        if (segLen <= 0f) segLen = Mathf.Max(cellSize.x, cellSize.y);

        int count = Mathf.Max(1, Mathf.CeilToInt(totalLength / segLen));
        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 pos = Vector3.Lerp(from, to, t);
            var seg = Object.Instantiate(roadPrefab, pos, Quaternion.identity, parent);
            seg.transform.right = unit;
        }
    }

    private static float MeasureRoadPrefabLength(GameObject roadPrefab)
    {
        if (roadPrefab == null) return 0f;
        float len = 0f;
        var tmp = Object.Instantiate(roadPrefab);
        var sr = tmp.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) len = sr.bounds.size.x;
        else
        {
            var mr = tmp.GetComponentInChildren<MeshRenderer>();
            if (mr != null) len = mr.bounds.size.x;
            else
            {
                var bc = tmp.GetComponentInChildren<BoxCollider2D>();
                if (bc != null) len = bc.size.x;
            }
        }
        Object.DestroyImmediate(tmp);
        return len;
    }
}
