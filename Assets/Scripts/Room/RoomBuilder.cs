using UnityEngine;

/// <summary>
/// Static utility to build a square room from prefabs: floor, walls and doors.
/// Use RoomBuilder.Build(parent, size, ...) to create children under the given parent transform.
/// </summary>
public static class RoomBuilder
{
    public static void Build(Transform parent, float size, GameObject floorPrefab, GameObject wallPrefab, GameObject doorPrefab, float wallThickness, float doorOutsideOffset, bool clearExistingChildren)
    {
        if (parent == null) return;

        // Clear previous builder-generated children according to mode
        ClearChildren(parent, clearExistingChildren);

        // create floor
        GameObject floor = null;
        if (floorPrefab != null)
        {
            floor = Object.Instantiate(floorPrefab, parent);
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
        float doorWidth = 1f;
        if (doorPrefab != null)
        {
            var dr = doorPrefab.GetComponentInChildren<Renderer>();
            if (dr != null)
            {
                doorWidth = dr.bounds.size.y;
                if (doorWidth <= 0f) doorWidth = 1f;
            }
        }

        // Horizontal walls (north/south): split by doorWidth
        float halfGapH = Mathf.Max(0f, (size - doorWidth) / 2f);
        float leftLenH = halfGapH + wallThickness;
        float rightLenH = leftLenH;

        // North
        CreateWall(parent, new Vector2(-(doorWidth / 2f + leftLenH / 2f), (size / 2f) + (wallThickness / 2f)), new Vector2(leftLenH, wallThickness), 0f, "Wall_North_Left", wallPrefab);
        CreateWall(parent, new Vector2((doorWidth / 2f + rightLenH / 2f), (size / 2f) + (wallThickness / 2f)), new Vector2(rightLenH, wallThickness), 0f, "Wall_North_Right", wallPrefab);

        // South
        CreateWall(parent, new Vector2(-(doorWidth / 2f + leftLenH / 2f), -(size / 2f) - (wallThickness / 2f)), new Vector2(leftLenH, wallThickness), 0f, "Wall_South_Left", wallPrefab);
        CreateWall(parent, new Vector2((doorWidth / 2f + rightLenH / 2f), -(size / 2f) - (wallThickness / 2f)), new Vector2(rightLenH, wallThickness), 0f, "Wall_South_Right", wallPrefab);

        // Vertical walls (east/west): need door height
        float doorHeight = 1f;
        if (doorPrefab != null)
        {
            var dr = doorPrefab.GetComponentInChildren<Renderer>();
            if (dr != null)
            {
                doorHeight = dr.bounds.size.y;
                if (doorHeight <= 0f) doorHeight = 1f;
            }
        }

        float halfGapV = Mathf.Max(0f, (size - doorHeight) / 2f);
        float leftLenV = halfGapV + wallThickness;
        float rightLenV = leftLenV;

        CreateWall(parent, new Vector2((size / 2f) + (wallThickness / 2f), -(doorHeight / 2f + leftLenV / 2f)), new Vector2(wallThickness, leftLenV), 0f, "Wall_East_Bottom", wallPrefab);
        CreateWall(parent, new Vector2((size / 2f) + (wallThickness / 2f), (doorHeight / 2f + rightLenV / 2f)), new Vector2(wallThickness, rightLenV), 0f, "Wall_East_Top", wallPrefab);

        CreateWall(parent, new Vector2(-(size / 2f) - (wallThickness / 2f), -(doorHeight / 2f + leftLenV / 2f)), new Vector2(wallThickness, leftLenV), 0f, "Wall_West_Bottom", wallPrefab);
        CreateWall(parent, new Vector2(-(size / 2f) - (wallThickness / 2f), (doorHeight / 2f + rightLenV / 2f)), new Vector2(wallThickness, rightLenV), 0f, "Wall_West_Top", wallPrefab);

        // create doors at midpoint of each wall (place in front)
        if (doorPrefab != null)
        {
            CreateDoor(parent, new Vector3(0f, size / 2f + doorOutsideOffset, 0f), 90f, "Door_North", doorPrefab);
            CreateDoor(parent, new Vector3(0f, -size / 2f - doorOutsideOffset, 0f), -90f, "Door_South", doorPrefab);
            CreateDoor(parent, new Vector3(size / 2f + doorOutsideOffset, 0f, 0f), 0f, "Door_East", doorPrefab);
            CreateDoor(parent, new Vector3(-size / 2f - doorOutsideOffset, 0f, 0f), 0f, "Door_West", doorPrefab);
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
                if (c.GetComponentInChildren<EnemySpawner>() != null || c.GetComponent<EnemySpawner>() != null)
                {
                    continue;
                }

                if (c.GetComponent<RoomBuilderMarker>() == null)
                {
                    continue;
                }
            }

#if UNITY_EDITOR
            Object.DestroyImmediate(c);
#else
            Object.Destroy(c);
#endif
        }
    }

    private static void CreateWall(Transform parent, Vector2 localPos, Vector2 size, float zRot, string name, GameObject wallPrefab)
    {
        GameObject wall;
        if (wallPrefab != null) wall = Object.Instantiate(wallPrefab, parent);
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

    private static void CreateDoor(Transform parent, Vector3 localPos, float zRot, string name, GameObject doorPrefab)
    {
        var door = Object.Instantiate(doorPrefab, parent);
        door.name = name;
        door.transform.localPosition = localPos;
        door.transform.localRotation = Quaternion.Euler(0f, 0f, zRot);
        var sr = door.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder += 10;
        }
        door.transform.SetAsLastSibling();
        door.AddComponent<RoomBuilderMarker>();
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
