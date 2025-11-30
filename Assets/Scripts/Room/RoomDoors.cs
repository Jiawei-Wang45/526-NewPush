using System;
using UnityEngine;
using static RoomManager;

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
            if (n.StartsWith("Door_North")) rm.obstacleNorth = tt.gameObject;
            else if (n.StartsWith("Door_East")) rm.obstacleEast = tt.gameObject;
            else if (n.StartsWith("Door_South")) rm.obstacleSouth = tt.gameObject;
            else if (n.StartsWith("Door_West")) rm.obstacleWest = tt.gameObject;
        }
    }
    private static readonly int ObstacleNums= Enum.GetValues(typeof(RoomManager.ObstacleDirection)).Length;
    public static GameObject GetDoor(RoomManager rm, RoomManager.ObstacleDirection dir)
    {
        if (rm == null) return null;
        return rm.GetObstacle(dir);
    }

    //public static bool HasDoor(RoomManager rm, RoomManager.DoorDirection dir) => GetDoor(rm, dir) != null;

    //public static GameObject[] GetAllDoors(RoomManager rm) => new GameObject[] { rm.obstacleNorth, rm.obstacleEast, rm.obstacleSouth, rm.obstacleWest };

    public static RoomManager.DoorMode GetDoorMode(RoomManager rm, RoomManager.ObstacleDirection dir)
    {
        if (rm == null) return RoomManager.DoorMode.Normal;
        return rm.GetDoorMode(dir);
    }

    public static void SetDoorMode(RoomManager rm, RoomManager.ObstacleDirection dir, RoomManager.DoorMode mode)
    {
        if (rm == null) return;
        switch (dir)
        {
            case RoomManager.ObstacleDirection.North: rm.obstacleNorthMode = mode; break;
            case RoomManager.ObstacleDirection.East: rm.obstacleEastMode = mode; break;
            case RoomManager.ObstacleDirection.South: rm.obstacleSouthMode = mode; break;
            case RoomManager.ObstacleDirection.West: rm.obstacleWestMode = mode; break;
        }
    }
   
    public static void CloseDoors(RoomManager rm)
    {
        if (rm == null) return;
        
        // isf using Tilemap system, add wall tiles to close doors
        if (rm.useTilemapBuilder)
        {
            for (int i = 0; i < 4; i++)
            {
                var dir = (RoomManager.ObstacleDirection)i;
                var mode = GetDoorMode(rm, dir);
                if (mode != RoomManager.DoorMode.PermanentlyLocked)
                {
                    // get corresponding wall tile for direction
                    UnityEngine.Tilemaps.TileBase wallTile = null;
                    switch (dir)
                    {
                        case RoomManager.ObstacleDirection.North: wallTile = rm.wallTopRuleTile; break;
                        case RoomManager.ObstacleDirection.South: wallTile = rm.wallBottomRuleTile; break;
                        case RoomManager.ObstacleDirection.East: wallTile = rm.wallRightRuleTile; break;
                        case RoomManager.ObstacleDirection.West: wallTile = rm.wallLeftRuleTile; break;
                    }
                    // use shared Grid overload
                    if (rm.sharedGrid != null)
                    {
                        RoomTilemapBuilder.CloseDoor(rm.sharedGrid, rm.transform, dir, wallTile, 2, rm.defaultSize);
                    }
                    else
                    {
                        RoomTilemapBuilder.CloseDoor(rm.transform, dir, wallTile, 2, rm.defaultSize);
                    }
                }
            }
        }
        else
        {
            // use traditional sprite system
            for (int i = 0; i < ObstacleNums; i++)
            {
                var dir = (RoomManager.ObstacleDirection)i;
                var door = GetDoor(rm, dir);
                var mode = GetDoorMode(rm, dir);
                if (mode == RoomManager.DoorMode.Normal)
                {
                    if (door != null) door.SetActive(true);
                }
            }
        }
    }

    public static void OpenDoors(RoomManager rm)
    {
        if (rm == null) return;
        
        // if using Tilemap system, remove wall tiles to open doors
        if (rm.useTilemapBuilder)
        {
            for (int i = 0; i < 4; i++)
            {
                var dir = (RoomManager.ObstacleDirection)i;
                var mode = GetDoorMode(rm, dir);
                if (mode != RoomManager.DoorMode.PermanentlyLocked)
                {
                    // use shared Grid overload
                    if (rm.sharedGrid != null)
                    {
                        RoomTilemapBuilder.OpenDoor(rm.sharedGrid, rm.transform, dir, 2, rm.defaultSize);
                    }
                    else
                    {
                        RoomTilemapBuilder.OpenDoor(rm.transform, dir, 2, rm.defaultSize);
                    }
                }
            }
        }
        else
        {
            // use traditional sprite system
            for (int i = 0; i < ObstacleNums; i++)
            {
                var dir = (RoomManager.ObstacleDirection)i;
                var door = GetDoor(rm, dir);
                var mode = GetDoorMode(rm, dir);
                if (mode == RoomManager.DoorMode.Normal)
                {
                    if (door != null) door.SetActive(false);
                }
            }
        }
    }

    private static void CreateDoor(RoomManager rm, RoomManager.ObstacleDirection dir)
    {
        if (rm == null || rm.doorPrefab == null) return;

    }
    // Create a wall-block GameObject at the door position. Uses RoomManager's wallPrefab if available.
    //private static void CreateBlock(RoomManager rm, RoomManager.ObstacleDirection dir)
    //{
    //    if (rm == null || rm.wallPrefab == null) return;
    //    string blockName = $"Door_{dir}_Block";
    //    var parent = rm.transform;
    //    var existing = parent.Find(blockName);
    //    if (existing != null) return; // already present

    //    GameObject door = GetDoor(rm, dir);
    //    GameObject block = GameObject.Instantiate(rm.wallPrefab, rm.transform);
    //    block.name = blockName;
    //    if (door != null)
    //    {
    //        block.transform.localPosition = door.transform.localPosition;
    //        block.transform.localRotation = door.transform.localRotation;
    //    }
    //    // for safety purpose 
    //    else
    //    {
    //        var bc = rm.gameObject.GetComponent<BoxCollider2D>();
    //        if (bc != null)
    //        {
    //            Vector2 localPos = Vector2.zero;
    //            float halfX = bc.size.x * 0.5f;
    //            float halfY = bc.size.y * 0.5f;
    //            switch (dir)
    //            {
    //                case RoomManager.ObstacleDirection.North: localPos = new Vector2(0f, halfY); block.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); break;
    //                case RoomManager.ObstacleDirection.South: localPos = new Vector2(0f, -halfY); block.transform.localRotation = Quaternion.Euler(0f, 0f, -90f); break;
    //                case RoomManager.ObstacleDirection.East: localPos = new Vector2(halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
    //                case RoomManager.ObstacleDirection.West: localPos = new Vector2(-halfX, 0f); block.transform.localRotation = Quaternion.identity; break;
    //            }
    //            block.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
    //        }
    //    }
    //    var sr = block.GetComponentInChildren<SpriteRenderer>();
    //    if (sr != null) sr.sortingOrder += 5;
    //}

//    private static void RemoveBlock(RoomManager rm, RoomManager.ObstacleDirection dir)
//    {
//        if (rm == null) return;
//        string blockName = $"Door_{dir}_Block";
//        var t = rm.transform.Find(blockName);
//        if (t != null)
//        {
//#if UNITY_EDITOR
//            GameObject.DestroyImmediate(t.gameObject);
//#else
//            Object.Destroy(t.gameObject);
//#endif
//        }
//    }
}
