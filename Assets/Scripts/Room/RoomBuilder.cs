using System;
using UnityEditor.PackageManager;
using UnityEngine;

/// <summary>
/// Static utility to build a square room from prefabs: floor, walls and doors.
/// Use RoomBuilder.Build(parent, size, ...) to create children under the given parent transform.
/// </summary>
public static class RoomBuilder
{
    //private static readonly string[] doorNames = { "Door_North", "Door_East", "Door_South", "Door_West" };
    //private static readonly string[] blockNames = { "Block_North", "Block_East", "Block_South", "Block_West" };
    public static void Build(Transform parent, float size, GameObject floorPrefab, GameObject wallPrefab, GameObject doorPrefab, float wallThickness, float doorOutsideOffset, bool clearExistingChildren)
    {
        if (parent == null) return;

        // Clear previous builder-generated children according to mode
        ClearChildren(parent, clearExistingChildren);

        // create floor
        GameObject floor = null;
        if (floorPrefab != null)
        {
            floor = GameObject.Instantiate(floorPrefab, parent);
            floor.AddComponent<RoomBuilderMarker>();
        }
        else
        {
            floor = new GameObject("Floor");
            floor.transform.SetParent(parent, false);
            var sr = floor.AddComponent<SpriteRenderer>();
            sr.color = Color.gray;
            floor.AddComponent<RoomBuilderMarker>();
        }
        ScaleToSize(floor, new Vector2(size + wallThickness, size + wallThickness));
        floor.transform.localPosition = Vector3.zero;

        // measure door size in world units (fallback to 1f if missing)
        float doorLength = 1f;
        if (doorPrefab != null)
        {
            var dr = doorPrefab.GetComponentInChildren<Renderer>();
            if (dr != null)
            {
                doorLength = dr.bounds.size.y;
                if (doorLength <= 0f) doorLength = 1f;
            }
        }

        // Horizontal walls (north/south): split by doorWidth
        float halfGap = Mathf.Max(0f, (size - doorLength) / 2f);
        float leftLen = halfGap + wallThickness;
        float rightLen = leftLen;

        // North
        CreateWall(parent, new Vector2(-(doorLength / 2f + leftLen / 2f), (size / 2f) + (wallThickness / 2f)), new Vector2(leftLen, wallThickness), 0f, "Wall_North_Left", wallPrefab);
        CreateWall(parent, new Vector2((doorLength / 2f + rightLen / 2f), (size / 2f) + (wallThickness / 2f)), new Vector2(rightLen, wallThickness), 0f, "Wall_North_Right", wallPrefab);

        // South
        CreateWall(parent, new Vector2(-(doorLength / 2f + leftLen / 2f), -(size / 2f) - (wallThickness / 2f)), new Vector2(leftLen, wallThickness), 0f, "Wall_South_Left", wallPrefab);
        CreateWall(parent, new Vector2((doorLength / 2f + rightLen / 2f), -(size / 2f) - (wallThickness / 2f)), new Vector2(rightLen, wallThickness), 0f, "Wall_South_Right", wallPrefab);

        // Vertical walls (east/west): need door height
 
        CreateWall(parent, new Vector2((size / 2f) + (wallThickness / 2f), -(doorLength / 2f + leftLen / 2f)), new Vector2(wallThickness, leftLen), 0f, "Wall_East_Bottom", wallPrefab);
        CreateWall(parent, new Vector2((size / 2f) + (wallThickness / 2f), (doorLength / 2f + rightLen / 2f)), new Vector2(wallThickness, rightLen), 0f, "Wall_East_Top", wallPrefab);

        CreateWall(parent, new Vector2(-(size / 2f) - (wallThickness / 2f), -(doorLength / 2f + leftLen / 2f)), new Vector2(wallThickness, leftLen), 0f, "Wall_West_Bottom", wallPrefab);
        CreateWall(parent, new Vector2(-(size / 2f) - (wallThickness / 2f), (doorLength / 2f + rightLen / 2f)), new Vector2(wallThickness, rightLen), 0f, "Wall_West_Top", wallPrefab);

        // delay creating doors since we may not need it
        //if (doorPrefab != null)
        //{
        //    CreateDoor(parent, new Vector3(0f, size / 2f + doorOutsideOffset, 0f), 90f, "Door_North", doorPrefab);
        //    CreateDoor(parent, new Vector3(0f, -size / 2f - doorOutsideOffset, 0f), -90f, "Door_South", doorPrefab);
        //    CreateDoor(parent, new Vector3(size / 2f + doorOutsideOffset, 0f, 0f), 0f, "Door_East", doorPrefab);
        //    CreateDoor(parent, new Vector3(-size / 2f - doorOutsideOffset, 0f, 0f), 0f, "Door_West", doorPrefab);
        //}
        RoomManager rm = parent.gameObject.GetComponent<RoomManager>();
        int ObstacleNums = Enum.GetValues(typeof(RoomManager.ObstacleDirection)).Length;
        for (int i = 0; i < ObstacleNums; i++)
        {
            var dir = (RoomManager.ObstacleDirection)i;
            var mode = rm.GetDoorMode(dir);
            if (mode == RoomManager.DoorMode.PermanentlyLocked)
            {

                CreateObstacle(rm, dir, size / 2 + doorOutsideOffset, $"Block_{dir}", wallPrefab);
            }
            else
            {
                CreateObstacle(rm, dir, size / 2 + doorOutsideOffset, $"Door_{dir}_Block", doorPrefab);
            }
        }
        // Create or update a BoxCollider2D on the parent to act as the room trigger.
        var existing = parent.gameObject.GetComponent<BoxCollider2D>();
        if (existing == null)
        {
            existing = parent.gameObject.AddComponent<BoxCollider2D>();
        }
        existing.isTrigger = true;
        existing.offset = Vector2.zero;
        existing.size = new Vector2(size, size);
    }

    private static void ClearChildren(Transform parent, bool clearExistingChildren)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var c = parent.GetChild(i).gameObject;
            if (!clearExistingChildren)
            {
                if (c.GetComponentInChildren<EnemySpawner>(true) != null)
                {
                    continue;
                }

                if (c.GetComponent<RoomBuilderMarker>() == null)
                {
                    continue;
                }
            }

#if UNITY_EDITOR
            GameObject.DestroyImmediate(c);
#else
            Object.Destroy(c);
#endif
        }
    }

    private static void CreateWall(Transform parent,Vector2 localPos, Vector2 size, float zRot, string name, GameObject wallPrefab)
    {
        GameObject wall;
        if (wallPrefab != null) wall = GameObject.Instantiate(wallPrefab, parent);
        else
        {
            wall = new GameObject(name);
            wall.transform.SetParent(parent, false);
            var sr = wall.AddComponent<SpriteRenderer>();
            sr.color = Color.black;
        }
        wall.name = name;
        wall.transform.localPosition = new Vector3(localPos.x, localPos.y, 0f);
        wall.transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
        ScaleToSize(wall, size);
        wall.AddComponent<RoomBuilderMarker>();
    }

    private static void CreateObstacle(RoomManager rm, RoomManager.ObstacleDirection dir, float offset, string name, GameObject obstaclePrefab)
    {
        if (!obstaclePrefab) return;
        Vector3 localPos=new Vector3();
        float zRot = 0;
        GameObject obstacle = GameObject.Instantiate(obstaclePrefab,rm.transform);
        switch (dir)
        {
            case RoomManager.ObstacleDirection.North:
                localPos = new Vector3(0, offset);
                zRot = 90;
                rm.obstacleNorth = obstacle;
                break;
            case RoomManager.ObstacleDirection.East:
                localPos = new Vector3(offset, 0);
                zRot = 0;
                rm.obstacleEast = obstacle;
                break;
            case RoomManager.ObstacleDirection.South:
                localPos = new Vector3(0, -offset);
                zRot = -90;
                rm.obstacleSouth = obstacle;
                break;
            case RoomManager.ObstacleDirection.West:
                localPos = new Vector3(-offset, 0);
                zRot = 0;
                rm.obstacleWest = obstacle;
                break;
        }
        obstacle.name = name;
        obstacle.transform.localPosition = localPos;
        obstacle.transform.localRotation= Quaternion.Euler(0f, 0f, zRot);
        var sr = obstacle.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder += 10;
        }
        //obstacle.transform.SetAsLastSibling();
        obstacle.AddComponent<RoomBuilderMarker>();
    }
    private static void ScaleToSize(GameObject go, Vector2 targetSize)
    {
        if (go == null) return;
        Renderer r = go.GetComponentInChildren<Renderer>();
        if (r != null)
        {
            var b = r.bounds.size;
            Vector3 lossy = r.transform.lossyScale;
            float origX = b.x / (lossy.x != 0f ? lossy.x : 1f);
            float origY = b.y / (lossy.y != 0f ? lossy.y : 1f);
            Vector3 newScale = go.transform.localScale;
            if (origX != 0f) newScale.x = targetSize.x / origX;
            if (origY != 0f) newScale.y = targetSize.y / origY;
            go.transform.localScale = newScale;
        }
        else
        {
            go.transform.localScale = new Vector3(targetSize.x, targetSize.y, 1f);
        }
    }
}

// Marker component placed on GameObjects created by RoomBuilder.
// Used so ClearChildren() can safely remove only builder-generated objects
// and leave designer-placed children intact.
public class RoomBuilderMarker : MonoBehaviour { }
