using UnityEngine;
using UnityEngine.Tilemaps;

// used to build rectangular rooms using Tilemaps and RuleTiles
// Grid cell size: 4x4x0
// Floor Order in Layer: -1
// Wall Order in Layer: 1 (with TilemapCollider2D)
public static class RoomTilemapBuilder
{
        // builds a rectangular room using Tilemaps and RuleTiles
        public static void BuildTilemapRoom(Grid sharedGrid, Vector3 worldPosition, float size, TileBase floorTile,
            TileBase wallTopTile, TileBase wallLeftTile, TileBase wallRightTile, TileBase wallBottomTile,
            TileBase innerTopTile = null, TileBase innerBottomTile = null, TileBase innerLeftTile = null,
            TileBase innerRightTile = null, TileBase innerBottomLeftTile = null, TileBase innerBottomRightTile = null, TileBase fillTile = null,
            Transform roomTransform = null,
            bool hasConnectionNorth = false, bool hasConnectionEast = false, bool hasConnectionSouth = false, bool hasConnectionWest = false)
        {
                // If no shared Grid is provided, create one (for backward compatibility)
                Grid grid = sharedGrid;
                if (grid == null && roomTransform != null)
                {
                        grid = roomTransform.GetComponentInChildren<Grid>();
                        if (grid == null)
                        {
                                GameObject gridObj = new GameObject("Grid");
                                gridObj.transform.SetParent(roomTransform, false);
                                gridObj.transform.localPosition = Vector3.zero;
                                grid = gridObj.AddComponent<Grid>();
                                grid.cellSize = new Vector3(4, 4, 0);
                        }
                }

                // Find or create Floor Tilemap
                Tilemap floorTilemap = FindOrCreateTilemap(grid.transform, "Floor", -1, false);

                // Find or create 4 separate Wall Tilemaps
                Tilemap wallTopTilemap = FindOrCreateTilemap(grid.transform, "WallTop", 1, true);
                Tilemap wallLeftTilemap = FindOrCreateTilemap(grid.transform, "WallLeft", 1, true);
                Tilemap wallRightTilemap = FindOrCreateTilemap(grid.transform, "WallRight", 1, true);
                Tilemap wallBottomTilemap = FindOrCreateTilemap(grid.transform, "WallBottom", 1, true);

                // Find or create Inner corner and Fill Tilemaps
                Tilemap innerTopTilemap = FindOrCreateTilemap(grid.transform, "InnerTop", 0, false);
                Tilemap innerBottomTilemap = FindOrCreateTilemap(grid.transform, "InnerBottom", 0, false);
                Tilemap innerLeftTilemap = FindOrCreateTilemap(grid.transform, "InnerLeft", 0, false);
                Tilemap innerRightTilemap = FindOrCreateTilemap(grid.transform, "InnerRight", 0, false);
                Tilemap innerBottomLeftTilemap = FindOrCreateTilemap(grid.transform, "InnerBottomLeft", 0, false);
                Tilemap innerBottomRightTilemap = FindOrCreateTilemap(grid.transform, "InnerBottomRight", 0, false);
                Tilemap fillTilemap = FindOrCreateTilemap(grid.transform, "Fill", 0, false);

                // Calculate how many tiles are needed to fill the room
                // Room size is in world units, each tile is 4x4
                int tilesPerSide = Mathf.CeilToInt(size / 4f);
                int halfTiles = tilesPerSide / 2;

                // Convert world position to grid cell coordinates
                Vector3Int centerCell = grid.WorldToCell(worldPosition);

                // Fill floor (entire room area)
                if (floorTile != null)
                {
                        for (int x = -halfTiles; x <= halfTiles; x++)
                        {
                                for (int y = -halfTiles; y <= halfTiles; y++)
                                {
                                        Vector3Int tilePos = centerCell + new Vector3Int(x, y, 0);
                                        floorTilemap.SetTile(tilePos, floorTile);
                                }
                        }
                }
                else
                {
                        Debug.LogError("[BuildTilemapRoom] floorTile is NULL!");
                }

                // Fill walls - generate walls along the floor boundary
                // Iterate over a larger area around the floor to detect where walls are needed
                int wallCount = 0;
                int doorWidth = 1; // Half-width of door opening (in tiles), skip [-doorWidth, doorWidth] which is 3 tiles

                // Expand detection range: check positions around the floor area
                int searchRadius = 3; // Search outward 3 tiles
                for (int x = -halfTiles - searchRadius; x <= halfTiles + searchRadius; x++)
                {
                        for (int y = -halfTiles - searchRadius; y <= halfTiles + searchRadius; y++)
                        {
                                Vector3Int checkPos = centerCell + new Vector3Int(x, y, 0);

                                // current position must not have floor
                                if (floorTilemap.GetTile(checkPos) != null) continue;

                                // check if at least one of the 8 surrounding tiles has floor
                                bool hasFloorNorth = floorTilemap.GetTile(checkPos + Vector3Int.up) != null;
                                bool hasFloorSouth = floorTilemap.GetTile(checkPos + Vector3Int.down) != null;
                                bool hasFloorEast = floorTilemap.GetTile(checkPos + Vector3Int.right) != null;
                                bool hasFloorWest = floorTilemap.GetTile(checkPos + Vector3Int.left) != null;
                                bool hasFloorNE = floorTilemap.GetTile(checkPos + new Vector3Int(1, 1, 0)) != null;
                                bool hasFloorNW = floorTilemap.GetTile(checkPos + new Vector3Int(-1, 1, 0)) != null;
                                bool hasFloorSE = floorTilemap.GetTile(checkPos + new Vector3Int(1, -1, 0)) != null;
                                bool hasFloorSW = floorTilemap.GetTile(checkPos + new Vector3Int(-1, -1, 0)) != null;

                                bool hasAnyAdjacentFloor = hasFloorNorth || hasFloorSouth || hasFloorEast || hasFloorWest ||
                                                          hasFloorNE || hasFloorNW || hasFloorSE || hasFloorSW;
                                // Check if ALL adjacent tiles are floor (a hole in the floor area). If so, this is a fill, not a wall.
                                bool hasAllAdjacentFloor = hasFloorNorth && hasFloorSouth && hasFloorEast && hasFloorWest &&
                                                          hasFloorNE && hasFloorNW && hasFloorSE && hasFloorSW;

                                // If there is no floor around, it means it's too far from the room, skip
                                if (!hasAnyAdjacentFloor) continue;

                                // If all surrounding tiles are floor, then this is a hole inside the floor, prioritize filling with fill tile
                                if (hasAllAdjacentFloor)
                                {
                                        if (fillTile != null)
                                        {
                                                fillTilemap.SetTile(checkPos, fillTile);
                                        }
                                        // If there is no fill tile, skip placing wall
                                        continue;
                                }

                                // Now this position needs a wall, determine which tile to use
                                // Determine if it is an outer wall or an inner corner
                                // Inner corner characteristic: three directions have floor, one corner direction does not have floor

                                Tilemap targetTilemap = null;
                                TileBase targetTile = null;

                                // Determine inner corner mode
                                // Top-left inner corner: floor to the east, south, and southeast
                                // First check top/bottom inner corners
                                // innerTop: floor to the left, right, and south but not north
                                if (hasFloorWest && hasFloorEast && hasFloorSouth && !hasFloorNorth && innerTopTile != null)
                                {
                                        targetTilemap = innerTopTilemap;
                                        targetTile = innerTopTile;
                                }
                                // innerBottom: floor to the left, right, and north but not south
                                else if (hasFloorWest && hasFloorEast && hasFloorNorth && !hasFloorSouth && innerBottomTile != null)
                                {
                                        targetTilemap = innerBottomTilemap;
                                        targetTile = innerBottomTile;
                                }

                                // Then check left/right inner corners
                                if (hasFloorEast && hasFloorSouth && hasFloorSE && !hasFloorNorth && !hasFloorWest && innerLeftTile != null)
                                {
                                        targetTilemap = innerLeftTilemap;
                                        targetTile = innerLeftTile;
                                }
                                // Top-right inner corner: floor to the west, south, and southwest
                                else if (hasFloorWest && hasFloorSouth && hasFloorSW && !hasFloorNorth && !hasFloorEast && innerRightTile != null)
                                {
                                        targetTilemap = innerRightTilemap;
                                        targetTile = innerRightTile;
                                }
                                // Bottom-left inner corner: floor to the east, north, and northeast
                                else if (hasFloorEast && hasFloorNorth && hasFloorNE && !hasFloorSouth && !hasFloorWest && innerBottomLeftTile != null)
                                {
                                        targetTilemap = innerBottomLeftTilemap;
                                        targetTile = innerBottomLeftTile;
                                }
                                // Bottom-right inner corner: floor to the west, north, and northwest
                                else if (hasFloorWest && hasFloorNorth && hasFloorNW && !hasFloorSouth && !hasFloorEast && innerBottomRightTile != null)
                                {
                                        targetTilemap = innerBottomRightTilemap;
                                        targetTile = innerBottomRightTile;
                                }
                                // Otherwise it is an outer wall, determine based on the main direction
                                else
                                {
                                        // Calculate position relative to room center
                                        int relX = x;
                                        int relY = y;

                                        // Determine main direction
                                        bool isTopSide = relY > halfTiles;
                                        bool isBottomSide = relY < -halfTiles;
                                        bool isLeftSide = relX < -halfTiles;
                                        bool isRightSide = relX > halfTiles;

                                        // Outer wall corners (outside two boundaries at the same time)
                                        if ((isTopSide || isBottomSide) && (isLeftSide || isRightSide))
                                        {
                                                if (isTopSide && isLeftSide)
                                                {
                                                        targetTilemap = wallLeftTilemap;
                                                        targetTile = wallLeftTile;
                                                }
                                                else if (isTopSide && isRightSide)
                                                {
                                                        targetTilemap = wallRightTilemap;
                                                        targetTile = wallRightTile;
                                                }
                                                else if (isBottomSide && isLeftSide)
                                                {
                                                        targetTilemap = wallBottomTilemap;
                                                        targetTile = wallBottomTile;
                                                }
                                                else if (isBottomSide && isRightSide)
                                                {
                                                        targetTilemap = wallBottomTilemap;
                                                        targetTile = wallBottomTile;
                                                }
                                        }
                                        // Top boundary
                                        else if (isTopSide)
                                        {
                                                if (hasConnectionNorth && relX >= -doorWidth && relX <= doorWidth)
                                                        continue;
                                                targetTilemap = wallTopTilemap;
                                                targetTile = wallTopTile;
                                        }
                                        // Bottom boundary
                                        else if (isBottomSide)
                                        {
                                                if (hasConnectionSouth && relX >= -doorWidth && relX <= doorWidth)
                                                        continue;
                                                targetTilemap = wallBottomTilemap;
                                                targetTile = wallBottomTile;
                                        }
                                        // Left boundary
                                        else if (isLeftSide)
                                        {
                                                if (hasConnectionWest && relY >= -doorWidth && relY <= doorWidth)
                                                        continue;
                                                targetTilemap = wallLeftTilemap;
                                                targetTile = wallLeftTile;
                                        }
                                        // Right boundary
                                        else if (isRightSide)
                                        {
                                                if (hasConnectionEast && relY >= -doorWidth && relY <= doorWidth)
                                                        continue;
                                                targetTilemap = wallRightTilemap;
                                                targetTile = wallRightTile;
                                        }
                                }

                                // Place wall tile
                                if (targetTilemap != null && targetTile != null)
                                {
                                        targetTilemap.SetTile(checkPos, targetTile);
                                        wallCount++;
                                }
                        }
                }


                // Refresh tilemaps to apply RuleTile rules
                floorTilemap.RefreshAllTiles();
                wallTopTilemap.RefreshAllTiles();
                wallLeftTilemap.RefreshAllTiles();
                wallRightTilemap.RefreshAllTiles();
                wallBottomTilemap.RefreshAllTiles();

                // Create or update room trigger (BoxCollider2D) if roomTransform is provided
                if (roomTransform != null)
                {
                        BoxCollider2D roomTrigger = roomTransform.GetComponent<BoxCollider2D>();
                        if (roomTrigger == null)
                        {
                                roomTrigger = roomTransform.gameObject.AddComponent<BoxCollider2D>();
                        }
                        roomTrigger.isTrigger = true;
                        roomTrigger.size = new Vector2(size, size);
                        roomTrigger.offset = Vector2.zero;

                }
        }

        // Create a door on the wall (remove wall tiles) - using shared Grid
        public static void CreateDoor(Grid sharedGrid, Transform roomTransform, RoomManager.ObstacleDirection direction, int doorWidth = 2, float roomSize = 8f)
        {
                if (sharedGrid == null) return;

                // Find the corresponding wall tilemap based on direction (under Grid)
                Tilemap wallTilemap = null;
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                wallTilemap = sharedGrid.transform.Find("WallTop")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.South:
                                wallTilemap = sharedGrid.transform.Find("WallBottom")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.East:
                                wallTilemap = sharedGrid.transform.Find("WallRight")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.West:
                                wallTilemap = sharedGrid.transform.Find("WallLeft")?.GetComponent<Tilemap>();
                                break;
                }

                if (wallTilemap == null) return;

                // Calculate the grid coordinates of the room center
                Vector3Int centerCell = sharedGrid.WorldToCell(roomTransform.position);
                int tilesPerSide = Mathf.CeilToInt(roomSize / 4f);
                int halfTiles = tilesPerSide / 2;
                int halfWidth = doorWidth / 2;

                // Remove wall tiles to create a door based on direction
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(centerCell + new Vector3Int(i, halfTiles + 1, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.South:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(centerCell + new Vector3Int(i, -halfTiles - 1, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.East:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(centerCell + new Vector3Int(halfTiles + 1, i, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.West:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(centerCell + new Vector3Int(-halfTiles - 1, i, 0), null);
                                }
                                break;
                }
        }

        // Create a door on the wall
        public static void CreateDoor(Transform roomTransform, RoomManager.ObstacleDirection direction, int doorWidth = 2, float roomSize = 8f)
        {
                Grid grid = roomTransform.GetComponentInChildren<Grid>();
                if (grid == null) return;
                // Find the corresponding wall tilemap based on direction
                Tilemap wallTilemap = null;
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                wallTilemap = grid.transform.Find("WallTop")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.South:
                                wallTilemap = grid.transform.Find("WallBottom")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.East:
                                wallTilemap = grid.transform.Find("WallRight")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.West:
                                wallTilemap = grid.transform.Find("WallLeft")?.GetComponent<Tilemap>();
                                break;
                }

                if (wallTilemap == null) return;

                // use room position to calculate center cell
                int tilesPerSide = Mathf.CeilToInt(roomSize / 4f);
                int halfTiles = tilesPerSide / 2;
                int halfWidth = doorWidth / 2;

                // Remove wall tiles to create a door based on direction
                // Walls are one tile outside the floor boundary, so it's halfTiles+1
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(new Vector3Int(i, halfTiles + 1, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.South:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(new Vector3Int(i, -halfTiles - 1, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.East:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(new Vector3Int(halfTiles + 1, i, 0), null);
                                }
                                break;

                        case RoomManager.ObstacleDirection.West:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                {
                                        wallTilemap.SetTile(new Vector3Int(-halfTiles - 1, i, 0), null);
                                }
                                break;
                }

                wallTilemap.RefreshAllTiles();
        }

        // close door 
        public static void CloseDoor(Grid sharedGrid, Transform roomTransform, RoomManager.ObstacleDirection direction, TileBase wallTile, int doorWidth = 2, float roomSize = 8f)
        {
                if (sharedGrid == null || wallTile == null) return;

                // Find the corresponding wall tilemap under the Grid
                Tilemap wallTilemap = null;
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                wallTilemap = sharedGrid.transform.Find("WallTop")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.South:
                                wallTilemap = sharedGrid.transform.Find("WallBottom")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.East:
                                wallTilemap = sharedGrid.transform.Find("WallRight")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.West:
                                wallTilemap = sharedGrid.transform.Find("WallLeft")?.GetComponent<Tilemap>();
                                break;
                }

                if (wallTilemap == null) return;

                Vector3Int centerCell = sharedGrid.WorldToCell(roomTransform.position);
                int tilesPerSide = Mathf.CeilToInt(roomSize / 4f);
                int halfTiles = tilesPerSide / 2;
                int halfWidth = doorWidth / 2;

                // Place wall tiles at the outer wall position (halfTiles+1)
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(centerCell + new Vector3Int(i, halfTiles + 1, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.South:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(centerCell + new Vector3Int(i, -halfTiles - 1, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.East:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(centerCell + new Vector3Int(halfTiles + 1, i, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.West:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(centerCell + new Vector3Int(-halfTiles - 1, i, 0), wallTile);
                                break;
                }

                wallTilemap.RefreshAllTiles();
        }

        // close door  
        public static void CloseDoor(Transform roomTransform, RoomManager.ObstacleDirection direction, TileBase wallTile, int doorWidth = 2, float roomSize = 8f)
        {
                Grid grid = roomTransform.GetComponentInChildren<Grid>();
                if (grid == null) return;

                Tilemap wallTilemap = null;
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                wallTilemap = grid.transform.Find("WallTop")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.South:
                                wallTilemap = grid.transform.Find("WallBottom")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.East:
                                wallTilemap = grid.transform.Find("WallRight")?.GetComponent<Tilemap>();
                                break;
                        case RoomManager.ObstacleDirection.West:
                                wallTilemap = grid.transform.Find("WallLeft")?.GetComponent<Tilemap>();
                                break;
                }

                if (wallTilemap == null || wallTile == null) return;

                int tilesPerSide = Mathf.CeilToInt(roomSize / 4f);
                int halfTiles = tilesPerSide / 2;
                int halfWidth = doorWidth / 2;

                // Place wall tiles at the outer wall position  
                switch (direction)
                {
                        case RoomManager.ObstacleDirection.North:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(new Vector3Int(i, halfTiles + 1, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.South:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(new Vector3Int(i, -halfTiles - 1, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.East:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(new Vector3Int(halfTiles + 1, i, 0), wallTile);
                                break;
                        case RoomManager.ObstacleDirection.West:
                                for (int i = -halfWidth; i <= halfWidth; i++)
                                        wallTilemap.SetTile(new Vector3Int(-halfTiles - 1, i, 0), wallTile);
                                break;
                }

                wallTilemap.RefreshAllTiles();
        }

        // open door  - using shared Grid
        public static void OpenDoor(Grid sharedGrid, Transform roomTransform, RoomManager.ObstacleDirection direction, int doorWidth = 2, float roomSize = 8f)
        {
                CreateDoor(sharedGrid, roomTransform, direction, doorWidth, roomSize);
        }

        // open door  - equivalent to CreateDoor
        public static void OpenDoor(Transform roomTransform, RoomManager.ObstacleDirection direction, int doorWidth = 2, float roomSize = 8f)
        {
                CreateDoor(roomTransform, direction, doorWidth, roomSize);
        }
        // build a road connecting two rooms
        public static void BuildTilemapRoad(Grid sharedGrid, Transform roadTransform, Vector3 from, Vector3 to, TileBase floorTile,
            TileBase wallTopTile, TileBase wallLeftTile, TileBase wallRightTile, TileBase wallBottomTile, int roadWidth = 2)
        {
                // Using shared Grid, no longer creating a new Grid
                Grid grid = sharedGrid;

                // Find or create Floor Tilemap - attached under Grid (shared)
                Tilemap floorTilemap = FindOrCreateTilemap(grid.transform, "Floor", -1, false);

                // Find or create 4 separate Wall Tilemaps - attached under Grid (shared)
                Tilemap wallTopTilemap = FindOrCreateTilemap(grid.transform, "WallTop", 1, true);
                Tilemap wallLeftTilemap = FindOrCreateTilemap(grid.transform, "WallLeft", 1, true);
                Tilemap wallRightTilemap = FindOrCreateTilemap(grid.transform, "WallRight", 1, true);
                Tilemap wallBottomTilemap = FindOrCreateTilemap(grid.transform, "WallBottom", 1, true);

                // Calculate road direction and length
                Vector3 dir = (to - from).normalized;

                // Determine if the road is horizontal or vertical
                bool isHorizontal = Mathf.Abs(dir.x) > Mathf.Abs(dir.y);

                // Convert world coordinates to grid coordinates
                Vector3Int startCell = grid.WorldToCell(from);
                Vector3Int endCell = grid.WorldToCell(to);

                int tileCount = 0;
                int wallCount = 0;

                // First fill floor, then check connectivity
                if (isHorizontal)
                {
                        // Horizontal road
                        int minX = Mathf.Min(startCell.x, endCell.x);
                        int maxX = Mathf.Max(startCell.x, endCell.x);
                        int centerY = (startCell.y + endCell.y) / 2;
                        int halfWidth = roadWidth / 2;

                        // Fill floor
                        if (floorTile != null)
                        {
                                for (int x = minX; x <= maxX; x++)
                                {
                                        for (int y = centerY - halfWidth; y <= centerY + halfWidth; y++)
                                        {
                                                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                                                tileCount++;
                                        }
                                }

                                // Check and extend floor to ensure connectivity
                                // Check if start and end points need extension
                                ExtendFloorForConnectivity(floorTilemap, floorTile, new Vector3Int(minX, centerY, 0), Vector3Int.left, halfWidth);
                                ExtendFloorForConnectivity(floorTilemap, floorTile, new Vector3Int(maxX, centerY, 0), Vector3Int.right, halfWidth);
                        }

                        // Fill walls - generate walls based on floor detection strategy
                        if (wallTopTile != null && wallBottomTile != null)
                        {
                                for (int x = minX - 1; x <= maxX + 1; x++)
                                {
                                        for (int y = centerY - halfWidth - 1; y <= centerY + halfWidth + 1; y++)
                                        {
                                                Vector3Int checkPos = new Vector3Int(x, y, 0);

                                                // If the current position already has a floor, skip
                                                if (floorTilemap.GetTile(checkPos) != null) continue;

                                                // Check if there is a floor in the surrounding 8 tiles
                                                bool hasFloorNorth = floorTilemap.GetTile(checkPos + Vector3Int.up) != null;
                                                bool hasFloorSouth = floorTilemap.GetTile(checkPos + Vector3Int.down) != null;
                                                bool hasFloorEast = floorTilemap.GetTile(checkPos + Vector3Int.right) != null;
                                                bool hasFloorWest = floorTilemap.GetTile(checkPos + Vector3Int.left) != null;

                                                bool hasAnyAdjacentFloor = hasFloorNorth || hasFloorSouth || hasFloorEast || hasFloorWest;

                                                if (!hasAnyAdjacentFloor) continue;

                                                // Determine which tilemap's walls to place
                                                // Horizontal road: use top/bottom for upper/lower boundaries
                                                int relY = y - centerY;
                                                if (relY > halfWidth)
                                                {
                                                        wallTopTilemap.SetTile(checkPos, wallTopTile);
                                                        wallCount++;
                                                }
                                                else if (relY < -halfWidth)
                                                {
                                                        wallBottomTilemap.SetTile(checkPos, wallBottomTile);
                                                        wallCount++;
                                                }
                                        }
                                }
                        }
                }
                else
                {
                        // Vertical road
                        int minY = Mathf.Min(startCell.y, endCell.y);
                        int maxY = Mathf.Max(startCell.y, endCell.y);
                        int centerX = (startCell.x + endCell.x) / 2;
                        int halfWidth = roadWidth / 2;

                        // Fill floor
                        if (floorTile != null)
                        {
                                for (int y = minY; y <= maxY; y++)
                                {
                                        for (int x = centerX - halfWidth; x <= centerX + halfWidth; x++)
                                        {
                                                floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                                                tileCount++;
                                        }
                                }

                                // Check and extend floor to ensure connectivity
                                // Check if start and end points need extension
                                ExtendFloorForConnectivity(floorTilemap, floorTile, new Vector3Int(centerX, minY, 0), Vector3Int.down, halfWidth);
                                ExtendFloorForConnectivity(floorTilemap, floorTile, new Vector3Int(centerX, maxY, 0), Vector3Int.up, halfWidth);
                        }

                        // Fill walls - generate walls based on floor detection strategy
                        if (wallLeftTile != null && wallRightTile != null)
                        {
                                for (int y = minY - 1; y <= maxY + 1; y++)
                                {
                                        for (int x = centerX - halfWidth - 1; x <= centerX + halfWidth + 1; x++)
                                        {
                                                Vector3Int checkPos = new Vector3Int(x, y, 0);

                                                // If the current position already has a floor, skip
                                                if (floorTilemap.GetTile(checkPos) != null) continue;

                                                // Check if there is a floor in the surrounding 8 tiles
                                                bool hasFloorNorth = floorTilemap.GetTile(checkPos + Vector3Int.up) != null;
                                                bool hasFloorSouth = floorTilemap.GetTile(checkPos + Vector3Int.down) != null;
                                                bool hasFloorEast = floorTilemap.GetTile(checkPos + Vector3Int.right) != null;
                                                bool hasFloorWest = floorTilemap.GetTile(checkPos + Vector3Int.left) != null;

                                                bool hasAnyAdjacentFloor = hasFloorNorth || hasFloorSouth || hasFloorEast || hasFloorWest;

                                                if (!hasAnyAdjacentFloor) continue;

                                                // Determine which tilemap's walls to place
                                                // Vertical road: use left/right for left/right boundaries
                                                int relX = x - centerX;
                                                if (relX < -halfWidth)
                                                {
                                                        wallLeftTilemap.SetTile(checkPos, wallLeftTile);
                                                        wallCount++;
                                                }
                                                else if (relX > halfWidth)
                                                {
                                                        wallRightTilemap.SetTile(checkPos, wallRightTile);
                                                        wallCount++;
                                                }
                                        }
                                }
                        }
                }

                // Refresh tilemaps to apply RuleTile rules
                floorTilemap.RefreshAllTiles();
                wallTopTilemap.RefreshAllTiles();
                wallLeftTilemap.RefreshAllTiles();
                wallRightTilemap.RefreshAllTiles();
                wallBottomTilemap.RefreshAllTiles();
        }

        // Extend floor to ensure connectivity with room
        private static void ExtendFloorForConnectivity(Tilemap floorTilemap, TileBase floorTile, Vector3Int startPos, Vector3Int direction, int halfWidth)
        {
                // Check along the direction if the floor needs to be extended
                Vector3Int checkPos = startPos;
                int maxExtension = 5; // Extend up to 5 tiles

                for (int i = 0; i < maxExtension; i++)
                {
                        checkPos += direction;

                        // Check if this position and its surrounding vertical range already have floor (indicating connectivity)
                        bool hasConnection = false;

                        // Determine whether to check vertical/horizontal range based on direction
                        if (direction.x != 0) // Moving horizontally, check vertical range
                        {
                                for (int dy = -halfWidth; dy <= halfWidth; dy++)
                                {
                                        if (floorTilemap.GetTile(checkPos + new Vector3Int(0, dy, 0)) != null)
                                        {
                                                hasConnection = true;
                                                break;
                                        }
                                }

                                // If not connected, fill floor
                                if (!hasConnection)
                                {
                                        for (int dy = -halfWidth; dy <= halfWidth; dy++)
                                        {
                                                floorTilemap.SetTile(checkPos + new Vector3Int(0, dy, 0), floorTile);
                                        }
                                }
                                else
                                {
                                        // Already connected, stop extending
                                        break;
                                }
                        }
                        else if (direction.y != 0) // Moving vertically, check horizontal range
                        {
                                for (int dx = -halfWidth; dx <= halfWidth; dx++)
                                {
                                        if (floorTilemap.GetTile(checkPos + new Vector3Int(dx, 0, 0)) != null)
                                        {
                                                hasConnection = true;
                                                break;
                                        }
                                }

                                // If not connected, fill floor
                                if (!hasConnection)
                                {
                                        for (int dx = -halfWidth; dx <= halfWidth; dx++)
                                        {
                                                floorTilemap.SetTile(checkPos + new Vector3Int(dx, 0, 0), floorTile);
                                        }
                                }
                                else
                                {
                                        // Already connected, stop extending
                                        break;
                                }
                        }
                }
        }

        private static Tilemap FindOrCreateTilemap(Transform gridTransform, string name, int sortingOrder, bool addCollider)
        {
                Transform tilemapTransform = gridTransform.Find(name);
                GameObject tilemapObj;

                if (tilemapTransform == null)
                {
                        tilemapObj = new GameObject(name);
                        tilemapObj.transform.SetParent(gridTransform, false);
                }
                else
                {
                        tilemapObj = tilemapTransform.gameObject;
                }

                Tilemap tilemap = tilemapObj.GetComponent<Tilemap>();
                if (tilemap == null)
                {
                        tilemap = tilemapObj.AddComponent<Tilemap>();
                }

                TilemapRenderer renderer = tilemapObj.GetComponent<TilemapRenderer>();
                if (renderer == null)
                {
                        renderer = tilemapObj.AddComponent<TilemapRenderer>();
                }
                renderer.sortingOrder = sortingOrder;

                if (addCollider)
                {
                        TilemapCollider2D collider = tilemapObj.GetComponent<TilemapCollider2D>();
                        if (collider == null)
                        {
                                collider = tilemapObj.AddComponent<TilemapCollider2D>();
                                collider.usedByComposite = true;
                        }
                        
                        // Add Rigidbody2D
                        Rigidbody2D rb = tilemapObj.GetComponent<Rigidbody2D>();
                        if (rb == null)
                        {
                                rb = tilemapObj.AddComponent<Rigidbody2D>();
                                rb.bodyType = RigidbodyType2D.Static;
                        }
                        
                        // Add CompositeCollider2D for better performance
                        CompositeCollider2D compositeCollider = tilemapObj.GetComponent<CompositeCollider2D>();
                        if (compositeCollider == null)
                        {
                                compositeCollider = tilemapObj.AddComponent<CompositeCollider2D>();
                                compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
                                compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
                        }
                        
                        // Set Wall layer
                        tilemapObj.layer = LayerMask.NameToLayer("Wall") != -1 ? LayerMask.NameToLayer("Wall") : 6;
                }

                return tilemap;
        }
}
