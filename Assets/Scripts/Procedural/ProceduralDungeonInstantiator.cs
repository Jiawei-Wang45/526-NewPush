using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class ProceduralDungeonInstantiator
{
    // Static fields to pass tilemap info to road creation
    private static bool s_useTilemapSystem;
    private static TileBase s_floorRuleTile;
    private static TileBase s_wallTopRuleTile;
    private static TileBase s_wallLeftRuleTile;
    private static TileBase s_wallRightRuleTile;
    private static TileBase s_wallBottomRuleTile;
    private static TileBase s_innerTopRuleTile;
    private static TileBase s_innerBottomRuleTile;
    private static TileBase s_innerLeftRuleTile;
    private static TileBase s_innerRightRuleTile;
    private static TileBase s_innerBottomLeftRuleTile;
    private static TileBase s_innerBottomRightRuleTile;
    private static TileBase s_fillRuleTile;
    private static Color s_tilemapColor;
    private static Grid s_sharedGrid; // 共享 Grid

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
        Transform parent,
        bool useTilemapSystem = false,
        TileBase floorRuleTile = null,
        TileBase wallTopRuleTile = null,
        TileBase wallLeftRuleTile = null,
        TileBase wallRightRuleTile = null,
        TileBase wallBottomRuleTile = null,
        TileBase innerTopRuleTile = null,
        TileBase innerBottomRuleTile = null,
        TileBase innerLeftRuleTile = null,
        TileBase innerRightRuleTile = null,
        TileBase innerBottomLeftRuleTile = null,
        TileBase innerBottomRightRuleTile = null,
        TileBase fillRuleTile = null,
        Color? tilemapColor = null,
        Grid sharedGrid = null)
    {
        // Store tilemap parameters for road creation
        s_useTilemapSystem = useTilemapSystem;
        s_floorRuleTile = floorRuleTile;
        s_wallTopRuleTile = wallTopRuleTile;
        s_wallLeftRuleTile = wallLeftRuleTile;
        s_wallRightRuleTile = wallRightRuleTile;
        s_wallBottomRuleTile = wallBottomRuleTile;
        s_innerTopRuleTile = innerTopRuleTile;
        s_innerBottomRuleTile = innerBottomRuleTile;
        s_innerLeftRuleTile = innerLeftRuleTile;
        s_innerRightRuleTile = innerRightRuleTile;
        s_innerBottomLeftRuleTile = innerBottomLeftRuleTile;
        s_innerBottomRightRuleTile = innerBottomRightRuleTile;
        s_fillRuleTile = fillRuleTile;
        
        Color colorToUse = tilemapColor ?? Color.white;
        s_tilemapColor = colorToUse;
        s_sharedGrid = sharedGrid; // save shared Grid reference
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
            Vector3 worldPos = new Vector3(cell.x * cellSize.x, cell.y * cellSize.y, 0f) + parent.position + dungeonOffset;
            GameObject roomInstance = null;
            GameObject prefabToUse = null;
            if (i == graphResult.startIndex && startRoomPrefab != null) prefabToUse = startRoomPrefab;
            else if (i == graphResult.endIndex && endRoomPrefab != null) prefabToUse = endRoomPrefab;
            else if (roomPrefabs != null && roomPrefabs.Length > 0) prefabToUse = roomPrefabs[UnityEngine.Random.Range(0, roomPrefabs.Length)];

            // If NOT using tilemap system, avoid _Tilemap variants
            if (!useTilemapSystem && prefabToUse != null && prefabToUse.name.EndsWith("_Tilemap"))
            {
                string originalName = prefabToUse.name.Substring(0, prefabToUse.name.Length - "_Tilemap".Length);
                GameObject nonTilemapVariant = null;
                
                // Try to find non-tilemap version in the same array
                if (i == graphResult.startIndex && startRoomPrefab != null)
                {
                    nonTilemapVariant = FindTilemapVariant(roomPrefabs, originalName);
                }
                else if (i == graphResult.endIndex && endRoomPrefab != null)
                {
                    nonTilemapVariant = FindTilemapVariant(roomPrefabs, originalName);
                }
                else
                {
                    nonTilemapVariant = FindTilemapVariant(roomPrefabs, originalName);
                }
                
                if (nonTilemapVariant != null)
                {
                    prefabToUse = nonTilemapVariant;
                }
            }
            // If using tilemap system, try to find a _Tilemap variant of the prefab
            else if (useTilemapSystem && prefabToUse != null)
            {
                string prefabName = prefabToUse.name;
                if (!prefabName.EndsWith("_Tilemap"))
                {
                    // First try to find it in the same array (roomPrefabs, startRoomPrefab, endRoomPrefab)
                    GameObject tilemapVariant = null;
                    string tilemapVariantName = prefabName + "_Tilemap";
                    
                    // Check if we're looking at start/end room or regular room
                    if (i == graphResult.startIndex && startRoomPrefab != null)
                    {
                        // For start room, also check roomPrefabs array
                        tilemapVariant = FindTilemapVariant(roomPrefabs, tilemapVariantName);
                    }
                    else if (i == graphResult.endIndex && endRoomPrefab != null)
                    {
                        // For end room, also check roomPrefabs array
                        tilemapVariant = FindTilemapVariant(roomPrefabs, tilemapVariantName);
                    }
                    else
                    {
                        // For regular rooms, search in roomPrefabs array
                        tilemapVariant = FindTilemapVariant(roomPrefabs, tilemapVariantName);
                    }
                    
                    if (tilemapVariant != null)
                    {
                        prefabToUse = tilemapVariant;
                    }
                }
            }

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

            // Preserve existing EnemySpawner from prefab
            var existingSpawner = roomInstance.GetComponentInChildren<EnemySpawner>(true);

            // If using tilemap system and this is a tilemap prefab, move tilemaps to sharedGrid
            if (useTilemapSystem && sharedGrid != null && prefabToUse != null && prefabToUse.name.EndsWith("_Tilemap"))
            {
                MoveTilemapsToSharedGrid(roomInstance, sharedGrid, worldPos, colorToUse);
            }

            // Apply scale so any built geometry matches intended world size
            roomInstance.transform.localScale = Vector3.one * roomScale;

            //  calculate connection info (which directions have road/room connections)
            bool hasNorth = false, hasEast = false, hasSouth = false, hasWest = false;
            
            for (int d = 0; d < cardinalDirs.Length; d++)
            {
                var nbCell = cell + cardinalDirs[d];
                bool connected = false;
                if (cellToIndex.TryGetValue(nbCell, out int nbIndex))
                {
                    if (adjacencyGraph.TryGetValue(i, out var neighs) && neighs.Contains(nbIndex))
                    {
                        connected = true;
                    }
                }
                
                // set connection flags
                if (d == 0) hasNorth = connected;
                else if (d == 1) hasEast = connected;
                else if (d == 2) hasSouth = connected;
                else if (d == 3) hasWest = connected;
            }

            // Use RoomManager's settings and static RoomBuilder to create or update geometry
            if (useTilemapSystem && floorRuleTile != null && wallTopRuleTile != null)
            {
                // using Tilemap system
                rm.useTilemapBuilder = true;
                rm.floorRuleTile = floorRuleTile;
                rm.wallTopRuleTile = wallTopRuleTile;
                rm.wallLeftRuleTile = wallLeftRuleTile;
                rm.wallRightRuleTile = wallRightRuleTile;
                rm.wallBottomRuleTile = wallBottomRuleTile;
                
                // pass shared Grid, room world position, and connection info
                RoomTilemapBuilder.BuildTilemapRoom(sharedGrid, worldPos, rm.defaultSize, floorRuleTile, 
                    wallTopRuleTile, wallLeftRuleTile, wallRightRuleTile, wallBottomRuleTile,
                    s_innerTopRuleTile, s_innerBottomRuleTile, s_innerLeftRuleTile, s_innerRightRuleTile, s_innerBottomLeftRuleTile, s_innerBottomRightRuleTile, s_fillRuleTile,
                    roomInstance.transform, hasNorth, hasEast, hasSouth, hasWest);
                
                // apply color to room Grid
                if (i == 0) // only apply color for the first room to avoid redundant work
                {
                    ApplyColorToGridTilemaps(sharedGrid.transform, colorToUse);
                }
            }
            else
            {
                // use traditional sprite system
                RoomBuilder.Build(roomInstance.transform, rm.defaultSize, rm.floorPrefab, rm.wallPrefab, rm.doorPrefab, rm.wallThickness, rm.doorOutsideOffset, rm.clearExistingChildren);
            }
            
            // Ensure room trigger is properly set
            rm.roomTrigger = roomInstance.GetComponent<BoxCollider2D>();
            if (rm.roomTrigger == null)
            {
                rm.roomTrigger = roomInstance.GetComponentInChildren<BoxCollider2D>(true);
            }
            // set flag to prevent RoomManager.Start() from rebuilding geometry
            rm.isProcedurallyGenerated = true;
            
            // set door modes (based on previously calculated connection info)
            rm.SetDoorMode(RoomManager.ObstacleDirection.North, hasNorth ? RoomManager.DoorMode.Normal : RoomManager.DoorMode.PermanentlyLocked);
            rm.SetDoorMode(RoomManager.ObstacleDirection.East, hasEast ? RoomManager.DoorMode.Normal : RoomManager.DoorMode.PermanentlyLocked);
            rm.SetDoorMode(RoomManager.ObstacleDirection.South, hasSouth ? RoomManager.DoorMode.Normal : RoomManager.DoorMode.PermanentlyLocked);
            rm.SetDoorMode(RoomManager.ObstacleDirection.West, hasWest ? RoomManager.DoorMode.Normal : RoomManager.DoorMode.PermanentlyLocked);

            // set sharedGrid reference
            rm.sharedGrid = s_sharedGrid;

            // Ensure doors exist and apply open/closed state immediately

            // Restore EnemySpawner reference if it exists in the prefab
            if (existingSpawner != null)
            {
                rm.enemySpawner = existingSpawner;
                // Ensure spawner has a gameManager reference
                if (existingSpawner.gameManager == null && GameManager.instance != null)
                {
                    existingSpawner.gameManager = GameManager.instance;
                }
            }

            instantiatedRooms.Add(roomInstance);
        }

        // Create roads
        if (roadPrefab != null || useTilemapSystem)
        {
            for (int i = 0; i < occupiedCells.Count; i++)
            {
                if (!adjacencyGraph.ContainsKey(i)) continue;
                foreach (var j in adjacencyGraph[i])
                {
                    if (j < i) continue;
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
        Vector3 aPos = GetEndpointForConnection(instantiatedRooms, occupiedCells, indexA, indexB,cellSize);
        Vector3 bPos = GetEndpointForConnection(instantiatedRooms, occupiedCells, indexB, indexA,cellSize);
        
        if (s_useTilemapSystem && s_floorRuleTile != null && s_wallTopRuleTile != null)
        {
            // using Tilemap system to create roads
            GameObject roadObj = new GameObject($"Road_{indexA}_{indexB}");
            roadObj.transform.SetParent(parent, false);
            roadObj.transform.position = Vector3.zero; // Grid must be at origin, otherwise WorldToCell will be incorrect

            // If endpoints are effectively equal (zero-length road), attempt to recompute endpoints
            if (Vector3.Distance(aPos, bPos) < 0.01f)
            {
                Vector3 centerA = instantiatedRooms[indexA] != null ? instantiatedRooms[indexA].transform.position : new Vector3(aCell.x * cellSize.x, aCell.y * cellSize.y, 0f);
                Vector3 centerB = instantiatedRooms[indexB] != null ? instantiatedRooms[indexB].transform.position : new Vector3(bCell.x * cellSize.x, bCell.y * cellSize.y, 0f);
                Vector3 delta = centerB - centerA;
                    if (delta.magnitude < 0.01f)
                {
                        // fallback to sprite-based road if possible
                        if (roadPrefab != null)
                        {
                            TileRoadBetween(aPos, bPos, roadPrefab, cellSize, parent);
                        }
                }
                else
                {
                    bool horizontal = Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
                    if (horizontal)
                    {
                        float halfX = cellSize.x * 0.5f;
                        aPos = centerA + new Vector3(Mathf.Sign(delta.x) * halfX, 0f, 0f);
                        bPos = centerB + new Vector3(-Mathf.Sign(delta.x) * halfX, 0f, 0f);
                    }
                    else
                    {
                        float halfY = cellSize.y * 0.5f;
                        aPos = centerA + new Vector3(0f, Mathf.Sign(delta.y) * halfY, 0f);
                        bPos = centerB + new Vector3(0f, -Mathf.Sign(delta.y) * halfY, 0f);
                    }
                    // If recomputation still resulted in a zero-length road, fallback to sprite-based road
                    if (Vector3.Distance(aPos, bPos) < 0.01f)
                    {
                        if (roadPrefab != null)
                        {
                            TileRoadBetween(aPos, bPos, roadPrefab, cellSize, parent);
                        }
                        return;
                    }
                }
            }
            RoomTilemapBuilder.BuildTilemapRoad(s_sharedGrid, roadObj.transform, aPos, bPos, s_floorRuleTile, 
                s_wallTopRuleTile, s_wallLeftRuleTile, s_wallRightRuleTile, s_wallBottomRuleTile, 2);
            
            // apply color to road Grid
            ApplyColorToGridTilemaps(roadObj.transform, s_tilemapColor);
        }
        else if (roadPrefab != null)
        {
            // use traditional sprite system
            TileRoadBetween(aPos, bPos, roadPrefab, cellSize, parent);
        }
    }

    private static Vector3 GetEndpointForConnection(List<GameObject> instantiatedRooms, List<Vector2Int> occupiedCells, int indexA, int indexB, Vector2 cellSize)
    {
        var room = instantiatedRooms[indexA];
        Vector2Int dir = occupiedCells[indexB] - occupiedCells[indexA];

        RoomManager.ObstacleDirection doorDir = RoomManager.ObstacleDirection.North;
        if (dir.x > 0) doorDir = RoomManager.ObstacleDirection.East;
        else if (dir.x < 0) doorDir = RoomManager.ObstacleDirection.West;
        else if (dir.y > 0) doorDir = RoomManager.ObstacleDirection.North;
        else if (dir.y < 0) doorDir = RoomManager.ObstacleDirection.South;

        // calculate door endpoint based on RoomManager door endpoint if available
        float roomSize = 8f; // default room size
        if (room != null)
        {
            var rm = room.GetComponent<RoomManager>();
            if (rm != null)
            {
                roomSize = rm.defaultSize;
                // if there is a door endpoint, use it
                if (rm.GetObstacle(doorDir)!=null)
                {
                    Vector3 doorEndpoint = rm.GetDoorEndpoint(doorDir);
                    return doorEndpoint;
                }
                
            }
        }
        // otherwise calculate door position (midpoint of room outer wall, aligned with outer wall)
        Vector3 center = room != null ? room.transform.position : new Vector3(occupiedCells[indexA].x * cellSize.x, occupiedCells[indexA].y * cellSize.y, 0f);
        Vector3 offset = Vector3.zero;
        float halfSize = roomSize * 0.5f;
        // door should be at room edge (outer wall position), no extra offset needed

        if (dir.x > 0) offset = new Vector3(halfSize, 0f, 0f); // East
        else if (dir.x < 0) offset = new Vector3(-halfSize, 0f, 0f); // West
        else if (dir.y > 0) offset = new Vector3(0f, halfSize, 0f); // North
        else if (dir.y < 0) offset = new Vector3(0f, -halfSize, 0f); // South

        Vector3 endpoint = center + offset;
        return endpoint;
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

    // Apply color to all TilemapRenderers under the Grid
    private static void ApplyColorToGridTilemaps(Transform roomTransform, Color color)
    {
        Grid grid = roomTransform.GetComponentInChildren<Grid>();
        if (grid == null) return;

        TilemapRenderer[] renderers = grid.GetComponentsInChildren<TilemapRenderer>();
        foreach (var renderer in renderers)
        {
            // TilemapRenderer uses material.color instead of a direct color property
            if (renderer.material != null)
            {
                renderer.material.color = color;
            }
        }
}

    // Find a tilemap variant by name in the prefabs array
    private static GameObject FindTilemapVariant(GameObject[] prefabs, string variantName)
    {
        if (prefabs == null || prefabs.Length == 0) return null;
        
        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null && prefab.name == variantName)
            {
                return prefab;
            }
        }
        return null;
    }

    // Move tilemaps from prefab's Grid to the shared Grid and apply color
    private static void MoveTilemapsToSharedGrid(GameObject roomInstance, Grid sharedGrid, Vector3 worldPos, Color color)
    {
        // Find the Grid in the prefab instance
        Grid prefabGrid = roomInstance.GetComponentInChildren<Grid>();
        if (prefabGrid == null) return;

        // Get all tilemaps under the prefab's Grid
        Tilemap[] tilemaps = prefabGrid.GetComponentsInChildren<Tilemap>();
        if (tilemaps.Length == 0) return;

        // Calculate the center cell position in the shared grid for this room
        Vector3Int roomCenterCell = sharedGrid.WorldToCell(worldPos);
        
        // Move each tilemap to the shared Grid by copying tiles
        foreach (Tilemap sourceTilemap in tilemaps)
        {
            string originalName = sourceTilemap.name;
            
            // Find or create the corresponding tilemap in the shared grid
            Tilemap targetTilemap = FindOrCreateSharedTilemap(sharedGrid, originalName, sourceTilemap, color);
            
            // Get all tiles from the source tilemap
            BoundsInt bounds = sourceTilemap.cellBounds;
            TileBase[] allTiles = sourceTilemap.GetTilesBlock(bounds);
            
            // Calculate offset: where the source tilemap's origin should map to in the target
            // The source tilemap is centered at (0,0) in prefab space
            // We want it to be centered at roomCenterCell in shared grid space
            Vector3Int boundsCenter = new Vector3Int(
                Mathf.RoundToInt(bounds.center.x),
                Mathf.RoundToInt(bounds.center.y),
                Mathf.RoundToInt(bounds.center.z)
            );
            Vector3Int offset = roomCenterCell - boundsCenter;
                        
            // Copy tiles to target tilemap with offset
            int tileCount = 0;
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    for (int z = 0; z < bounds.size.z; z++)
                    {
                        int index = x + y * bounds.size.x + z * bounds.size.x * bounds.size.y;
                        TileBase tile = allTiles[index];
                        if (tile != null)
                        {
                            Vector3Int sourcePos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, bounds.zMin + z);
                            Vector3Int targetPos = sourcePos + offset;
                            targetTilemap.SetTile(targetPos, tile);
                            tileCount++;
                        }
                    }
                }
            }            
            // Refresh the target tilemap to apply rule tiles
            targetTilemap.RefreshAllTiles();
        }

        // Destroy the now-empty Grid from the prefab instance
        Object.DestroyImmediate(prefabGrid.gameObject);
    }
    
    // Find or create a tilemap in the shared grid with matching properties
    private static Tilemap FindOrCreateSharedTilemap(Grid sharedGrid, string name, Tilemap sourceTilemap, Color color)
    {
        // Try to find existing tilemap with this name
        Transform existing = sharedGrid.transform.Find(name);
        if (existing != null)
        {
            Tilemap existingTilemap = existing.GetComponent<Tilemap>();
            if (existingTilemap != null) return existingTilemap;
        }
        
        // Create new tilemap GameObject
        GameObject tilemapObj = new GameObject(name);
        tilemapObj.transform.SetParent(sharedGrid.transform, false);
        tilemapObj.transform.localPosition = Vector3.zero;
        
        // Add Tilemap component
        Tilemap targetTilemap = tilemapObj.AddComponent<Tilemap>();
        
        // Add and configure TilemapRenderer
        TilemapRenderer renderer = tilemapObj.AddComponent<TilemapRenderer>();
        TilemapRenderer sourceRenderer = sourceTilemap.GetComponent<TilemapRenderer>();
        if (sourceRenderer != null)
        {
            renderer.sortingOrder = sourceRenderer.sortingOrder;
            renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        }
        if (renderer.material != null)
        {
            renderer.material.color = color;
        }
        
        // Check if source has collider and add to target if needed
        TilemapCollider2D sourceCollider = sourceTilemap.GetComponent<TilemapCollider2D>();
        if (sourceCollider != null)
        {
            TilemapCollider2D collider = tilemapObj.AddComponent<TilemapCollider2D>();
            collider.usedByComposite = true;
            // Add Rigidbody2D (required for CompositeCollider2D)
            Rigidbody2D rb = tilemapObj.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            
            // Add CompositeCollider2D for better performance
            CompositeCollider2D compositeCollider = tilemapObj.AddComponent<CompositeCollider2D>();
            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
            
            // Set Wall layer if it's a wall tilemap
            if (name.Contains("Wall"))
            {
                tilemapObj.layer = LayerMask.NameToLayer("Wall") != -1 ? LayerMask.NameToLayer("Wall") : 6;
            }
        }
        
        return targetTilemap;
    }
}
