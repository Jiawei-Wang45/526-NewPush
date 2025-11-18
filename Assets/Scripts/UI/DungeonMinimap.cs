using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dungeon minimap UI script that displays the complete dungeon layout including rooms and connections
/// </summary>
public class DungeonMinimap : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("Container for placing room icons and connections")]
    public RectTransform roomContainer;
    
    [Header("Room Icons")]
    [Tooltip("Icon prefab for visited rooms")]
    public GameObject roomIconPrefab;
    
    [Tooltip("Icon prefab for adjacent unvisited rooms")]
    public GameObject adjacentUnvisitedRoomIconPrefab;
    
    [Tooltip("Icon prefab for start room")]
    public GameObject startRoomIconPrefab;
    
    [Tooltip("Icon prefab for end room")]
    public GameObject endRoomIconPrefab;
    
    [Tooltip("Prefab for player location indicator (four-corner frame)")]
    public GameObject playerLocationIndicatorPrefab;
    
    [Header("Minimap Settings")]
    [Tooltip("Minimap scale factor (used to convert grid coordinates to UI coordinates)")]
    public float minimapScale = 0.1f;
    
    [Tooltip("Room icon size in UI units (if set to 0, uses prefab's original size)")]
    public float roomIconSize = 20f;
    
    [Header("Connection Settings")]
    [Tooltip("Whether to show connections (roads) between rooms")]
    public bool showConnections = true;
    
    [Tooltip("Connection line width")]
    public float connectionLineWidth = 2f;
    
    [Tooltip("Color for connection lines")]
    public Color connectionColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
    
    // Dictionary storing room icons
    private Dictionary<int, GameObject> roomIcons = new Dictionary<int, GameObject>();
    // List storing connection line objects
    private List<GameObject> connectionLines = new List<GameObject>();
    // Set of room indices that the player has visited
    private HashSet<int> visitedRooms = new HashSet<int>();
    // Set of room indices that were previously shown as adjacent unvisited (to detect state changes)
    private HashSet<int> previouslyAdjacentUnvisited = new HashSet<int>();
    // Last known current room index (to detect room changes)
    private int lastCurrentRoomIndex = -1;
    // Player location indicator (four-corner frame)
    private GameObject playerLocationIndicator = null;
    
    private void Awake()
    {
        // If no container is specified, try to use the current GameObject as the container
        if (roomContainer == null)
        {
            roomContainer = GetComponent<RectTransform>();
            if (roomContainer == null)
            {
                // If the current GameObject doesn't have a RectTransform, add one
                roomContainer = gameObject.AddComponent<RectTransform>();
            }
        }
        
        // Ensure roomContainer has a Canvas as parent (UI elements require Canvas)
        if (roomContainer != null && roomContainer.GetComponentInParent<Canvas>() == null)
        {
            Debug.LogWarning("DungeonMinimap: roomContainer is not under a Canvas! UI elements may not display correctly.");
        }
    }
    
    private void Start()
    {
        // Delayed initialization to ensure GameManager and rooms are ready
        StartCoroutine(DelayedInitialize());
    }
    
    private void OnEnable()
    {
        // Update minimap when UI is enabled
        if (GameManager.instance != null)
        {
            // If not yet initialized, initialize first
            if (roomIcons.Count == 0)
            {
                StartCoroutine(DelayedInitialize());
            }
            else
            {
                RefreshMinimap();
            }
        }
    }
    
    /// <summary>
    /// Delayed initialization, waiting for graph structure to be ready
    /// </summary>
    private System.Collections.IEnumerator DelayedInitialize()
    {
        yield return null;
        yield return null;
        
        int maxAttempts = 10;
        int attempts = 0;
        while (attempts < maxAttempts)
        {
            if (GameManager.instance != null && 
                GameManager.instance.rooms != null && 
                GameManager.instance.rooms.Length > 0 &&
                GameManager.instance.roomGridPositions != null && 
                GameManager.instance.roomGridPositions.Count > 0 &&
                GameManager.instance.dungeonCellSize != Vector2.zero)
            {
                InitializeMinimap();
                yield break;
            }
            attempts++;
            yield return new WaitForSeconds(0.1f);
        }
        
        Debug.LogError("DungeonMinimap: Failed to initialize - graph structure not found!");
    }
    
    /// <summary>
    /// Initialize minimap using graph structure
    /// </summary>
    public void InitializeMinimap()
    {
        ClearMinimap();
        
        var gridPositions = GameManager.instance.roomGridPositions;
        var cellSize = GameManager.instance.dungeonCellSize;
        var rooms = GameManager.instance.rooms;
        
        // Calculate center from grid positions
        Vector2 minGridPos = new Vector2(gridPositions[0].x, gridPositions[0].y);
        Vector2 maxGridPos = new Vector2(gridPositions[0].x, gridPositions[0].y);
        
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector2Int gridPos = gridPositions[i];
            minGridPos = Vector2.Min(minGridPos, new Vector2(gridPos.x, gridPos.y));
            maxGridPos = Vector2.Max(maxGridPos, new Vector2(gridPos.x, gridPos.y));
        }
        
        Vector2 centerGridPos = (minGridPos + maxGridPos) * 0.5f;
        
        // Calculate UI positions for all rooms
        Dictionary<int, Vector2> roomUIPositions = new Dictionary<int, Vector2>();
        for (int i = 0; i < rooms.Length; i++)
        {
            Vector2Int gridPos = gridPositions[i];
            Vector2 relativeGridPos = new Vector2(gridPos.x, gridPos.y) - centerGridPos;
            Vector2 uiPosition = new Vector2(relativeGridPos.x * cellSize.x * minimapScale, relativeGridPos.y * cellSize.y * minimapScale);
            roomUIPositions[i] = uiPosition;
        }
        
        // Initialize visited rooms with starting room first (before creating icons)
        int currentRoomIndex = GameManager.instance.GetCurrentRoomIndex();
        if (currentRoomIndex >= 0)
        {
            visitedRooms.Add(currentRoomIndex);
            lastCurrentRoomIndex = currentRoomIndex;
        }
        
        // Don't create icons in initialization - let UpdateMinimapDisplay create them as needed
        // This ensures correct prefabs are used from the start
        
        UpdateMinimapDisplay();
        
        // Initialize player location indicator
        if (currentRoomIndex >= 0)
        {
            UpdatePlayerLocationIndicator(currentRoomIndex);
        }
    }
    
    /// <summary>
    /// Refresh minimap - update visited rooms and visibility
    /// </summary>
    public void RefreshMinimap()
    {
        int currentRoomIndex = GameManager.instance.GetCurrentRoomIndex();
        bool roomChanged = currentRoomIndex != lastCurrentRoomIndex;
        
        if (currentRoomIndex >= 0)
        {
            if (!visitedRooms.Contains(currentRoomIndex))
            {
                visitedRooms.Add(currentRoomIndex);
            }
            
            if (roomChanged)
            {
                UpdateMinimapDisplay();
            }
            
            // Update player location indicator
            UpdatePlayerLocationIndicator(currentRoomIndex);
            
            lastCurrentRoomIndex = currentRoomIndex;
        }
        else
        {
            // Player is not in any room - keep showing indicator at last known room position
            if (lastCurrentRoomIndex >= 0)
            {
                // Keep showing indicator at the last known room
                UpdatePlayerLocationIndicator(lastCurrentRoomIndex);
            }
            // Only hide if we never had a valid room index
            else if (playerLocationIndicator != null)
            {
                playerLocationIndicator.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Update minimap display to show visited rooms, adjacent unvisited rooms, and their connections
    /// </summary>
    private void UpdateMinimapDisplay()
    {
        Dictionary<int, Vector2> allRoomUIPositions = CalculateAllRoomUIPositions();
        
        // Find adjacent unvisited rooms (rooms connected to visited rooms but not visited themselves)
        HashSet<int> adjacentUnvisitedRooms = new HashSet<int>();
        var adjacencyGraph = GameManager.instance.roomAdjacencyGraph;
        
        foreach (int visitedRoomIndex in visitedRooms)
        {
            if (adjacencyGraph.ContainsKey(visitedRoomIndex))
            {
                foreach (int connectedIndex in adjacencyGraph[visitedRoomIndex])
                {
                    if (!visitedRooms.Contains(connectedIndex))
                    {
                        adjacentUnvisitedRooms.Add(connectedIndex);
                    }
                }
            }
        }
        
        // Draw connections first (so they appear behind room icons)
        if (showConnections)
        {
            DrawVisitedRoomConnections(allRoomUIPositions);
        }
        
        // Update room icon visibility
        // Simple logic: visited rooms show with their appropriate prefab, adjacent unvisited rooms show with unvisited prefab
        var rooms = GameManager.instance.rooms;
        
        for (int i = 0; i < rooms.Length; i++)
        {
            bool isVisited = visitedRooms.Contains(i);
            bool isAdjacentUnvisited = adjacentUnvisitedRooms.Contains(i);
            bool shouldShow = isVisited || isAdjacentUnvisited;
            
            if (shouldShow)
            {
                // Check if room state changed: was adjacent unvisited, now visited
                bool wasAdjacentUnvisited = previouslyAdjacentUnvisited.Contains(i);
                bool needsRecreate = false;
                
                if (isVisited && wasAdjacentUnvisited)
                {
                    // Room was just visited (was adjacent unvisited before), need to recreate with visited prefab
                    needsRecreate = true;
                    previouslyAdjacentUnvisited.Remove(i);
                }
                
                // If icon doesn't exist or needs recreation, create it
                if (!roomIcons.ContainsKey(i) || roomIcons[i] == null || needsRecreate)
                {
                    if (needsRecreate && roomIcons.ContainsKey(i) && roomIcons[i] != null)
                    {
                        Destroy(roomIcons[i]);
                        roomIcons.Remove(i);
                    }
                    
                    if (allRoomUIPositions.ContainsKey(i))
                    {
                        GameObject icon = CreateRoomIcon(i, allRoomUIPositions[i], isAdjacentUnvisited && !isVisited);
                        if (icon != null)
                        {
                            roomIcons[i] = icon;
                            // Ensure room icons are rendered on top of connection lines
                            icon.transform.SetAsLastSibling();
                        }
                    }
                }
                else
                {
                    // Just show the icon and ensure it's on top
                    roomIcons[i].SetActive(true);
                    roomIcons[i].transform.SetAsLastSibling();
                }
                
                // Track adjacent unvisited rooms for next frame
                if (isAdjacentUnvisited && !isVisited)
                {
                    previouslyAdjacentUnvisited.Add(i);
                }
            }
            else
            {
                // Hide icon if it exists
                if (roomIcons.ContainsKey(i) && roomIcons[i] != null)
                {
                    roomIcons[i].SetActive(false);
                }
                // Remove from tracking if no longer adjacent unvisited
                previouslyAdjacentUnvisited.Remove(i);
            }
        }
    }
    
    /// <summary>
    /// Calculate UI positions for all rooms using graph structure
    /// </summary>
    private Dictionary<int, Vector2> CalculateAllRoomUIPositions()
    {
        Dictionary<int, Vector2> roomUIPositions = new Dictionary<int, Vector2>();
        
        var gridPositions = GameManager.instance.roomGridPositions;
        var cellSize = GameManager.instance.dungeonCellSize;
        var rooms = GameManager.instance.rooms;
        
        // Calculate center
        Vector2 minGridPos = new Vector2(gridPositions[0].x, gridPositions[0].y);
        Vector2 maxGridPos = new Vector2(gridPositions[0].x, gridPositions[0].y);
        
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector2Int gridPos = gridPositions[i];
            minGridPos = Vector2.Min(minGridPos, new Vector2(gridPos.x, gridPos.y));
            maxGridPos = Vector2.Max(maxGridPos, new Vector2(gridPos.x, gridPos.y));
        }
        
        Vector2 centerGridPos = (minGridPos + maxGridPos) * 0.5f;
        
        // Calculate UI positions
        for (int i = 0; i < rooms.Length; i++)
        {
            Vector2Int gridPos = gridPositions[i];
            Vector2 relativeGridPos = new Vector2(gridPos.x, gridPos.y) - centerGridPos;
            Vector2 uiPosition = new Vector2(relativeGridPos.x * cellSize.x * minimapScale, relativeGridPos.y * cellSize.y * minimapScale);
            roomUIPositions[i] = uiPosition;
        }
        
        return roomUIPositions;
    }
    
    /// <summary>
    /// Draw connections for all explored rooms (connections from visited rooms to any adjacent room)
    /// </summary>
    private void DrawVisitedRoomConnections(Dictionary<int, Vector2> allRoomUIPositions)
    {
        ClearConnections();
        
        var adjacencyGraph = GameManager.instance.roomAdjacencyGraph;
        HashSet<string> drawnConnections = new HashSet<string>();
        
        // Draw all connections from visited rooms (including to unvisited adjacent rooms)
        foreach (var kvp in adjacencyGraph)
        {
            int roomIndex = kvp.Key;
            if (!visitedRooms.Contains(roomIndex)) continue;
            
            Vector2 startPos = allRoomUIPositions[roomIndex];
            
            // Draw connections to all adjacent rooms (both visited and unvisited)
            foreach (int connectedIndex in kvp.Value)
            {
                string connectionKey = roomIndex < connectedIndex ? $"{roomIndex}-{connectedIndex}" : $"{connectedIndex}-{roomIndex}";
                if (drawnConnections.Contains(connectionKey)) continue;
                drawnConnections.Add(connectionKey);
                
                connectionLines.Add(CreateConnectionLine(startPos, allRoomUIPositions[connectedIndex]));
            }
        }
    }
    
    /// <summary>
    /// Get the correct prefab for a room based on its state
    /// </summary>
    private GameObject GetCorrectPrefabForRoom(int roomIndex, bool isVisited, bool isAdjacentUnvisited, bool isStartRoom, bool isEndRoom)
    {
        if (isStartRoom && isVisited && startRoomIconPrefab != null)
        {
            return startRoomIconPrefab;
        }
        if (isEndRoom && isVisited && endRoomIconPrefab != null)
        {
            return endRoomIconPrefab;
        }
        if (isAdjacentUnvisited && !isVisited && adjacentUnvisitedRoomIconPrefab != null)
        {
            return adjacentUnvisitedRoomIconPrefab;
        }
        if (roomIconPrefab != null)
        {
            return roomIconPrefab;
        }
        return null;
    }
    
    /// <summary>
    /// Create a room icon at the specified UI position using appropriate prefab
    /// </summary>
    private GameObject CreateRoomIcon(int roomIndex, Vector2 uiPosition, bool isAdjacentUnvisited = false)
    {
        GameObject prefabToUse = null;
        
        // Determine which prefab to use based on room type
        if (GameManager.instance == null) return null;
        
        int startRoomIndex = GameManager.instance.startRoomIndex;
        int endRoomIndex = GameManager.instance.endRoomIndex;
        bool isVisited = visitedRooms.Contains(roomIndex);
        
        // Priority: adjacent unvisited > start room (if visited) > end room (if visited) > normal visited
        if (isAdjacentUnvisited && !isVisited)
        {
            // Adjacent unvisited room
            if (adjacentUnvisitedRoomIconPrefab != null)
            {
                prefabToUse = adjacentUnvisitedRoomIconPrefab;
            }
        }
        else if (isVisited)
        {
            // Visited room - check if it's start or end room
            if (roomIndex == startRoomIndex && startRoomIconPrefab != null)
            {
                prefabToUse = startRoomIconPrefab;
            }
            else if (roomIndex == endRoomIndex && endRoomIconPrefab != null)
            {
                prefabToUse = endRoomIconPrefab;
            }
            else if (roomIconPrefab != null)
            {
                prefabToUse = roomIconPrefab;
            }
        }
        
        if (prefabToUse == null)
        {
            return null; // No prefab specified, don't create icon
        }
        
        GameObject icon = Instantiate(prefabToUse, roomContainer);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        if (iconRect == null)
        {
            iconRect = icon.AddComponent<RectTransform>();
        }
        
        // Set position
        iconRect.anchoredPosition = uiPosition;
        iconRect.localScale = Vector3.one;
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        
        // Apply size scaling if roomIconSize is set
        if (roomIconSize > 0)
        {
            iconRect.sizeDelta = new Vector2(roomIconSize, roomIconSize);
        }
        
        return icon;
    }
    
    
    /// <summary>
    /// Create a UI line between two positions
    /// </summary>
    private GameObject CreateConnectionLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObj = new GameObject("ConnectionLine");
        lineObj.transform.SetParent(roomContainer, false);
        
        // Ensure connection lines are rendered behind room icons
        lineObj.transform.SetAsFirstSibling();
        
        RectTransform rect = lineObj.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        // Calculate line properties
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        // Set position to midpoint
        rect.anchoredPosition = (startPos + endPos) * 0.5f;
        rect.sizeDelta = new Vector2(distance, connectionLineWidth);
        rect.localRotation = Quaternion.Euler(0, 0, angle);
        
        // Add image component for rendering
        Image lineImage = lineObj.AddComponent<Image>();
        lineImage.color = connectionColor;
        
        // Create a simple white texture for the line
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        lineImage.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        return lineObj;
    }
    
    /// <summary>
    /// Clear all connection lines
    /// </summary>
    private void ClearConnections()
    {
        foreach (var line in connectionLines)
        {
            if (line != null)
            {
                Destroy(line);
            }
        }
        connectionLines.Clear();
    }
    
    
    
    /// <summary>
    /// Update player location indicator to show current room
    /// </summary>
    private void UpdatePlayerLocationIndicator(int currentRoomIndex)
    {
        if (playerLocationIndicatorPrefab == null) return;
        
        Dictionary<int, Vector2> allRoomUIPositions = CalculateAllRoomUIPositions();
        
        if (!allRoomUIPositions.ContainsKey(currentRoomIndex)) return;
        
        Vector2 roomUIPosition = allRoomUIPositions[currentRoomIndex];
        
        // Create indicator if it doesn't exist
        if (playerLocationIndicator == null)
        {
            playerLocationIndicator = Instantiate(playerLocationIndicatorPrefab, roomContainer);
            RectTransform indicatorRect = playerLocationIndicator.GetComponent<RectTransform>();
            if (indicatorRect == null)
            {
                indicatorRect = playerLocationIndicator.AddComponent<RectTransform>();
            }
            
            // Set anchor and pivot to center
            indicatorRect.anchorMin = new Vector2(0.5f, 0.5f);
            indicatorRect.anchorMax = new Vector2(0.5f, 0.5f);
            indicatorRect.pivot = new Vector2(0.5f, 0.5f);
            indicatorRect.localScale = Vector3.one;
            
            // Apply size scaling if roomIconSize is set (same as room icons)
            if (roomIconSize > 0)
            {
                indicatorRect.sizeDelta = new Vector2(roomIconSize, roomIconSize);
            }
        }
        
        // Update position to current room
        RectTransform rect = playerLocationIndicator.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = roomUIPosition;
            // Ensure indicator is rendered on top of room icons
            playerLocationIndicator.transform.SetAsLastSibling();
        }
        
        // Show indicator
        playerLocationIndicator.SetActive(true);
    }
    
    /// <summary>
    /// Clear all icons and connections on the minimap
    /// </summary>
    private void ClearMinimap()
    {
        foreach (var icon in roomIcons.Values)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        roomIcons.Clear();
        
        if (playerLocationIndicator != null)
        {
            Destroy(playerLocationIndicator);
            playerLocationIndicator = null;
        }
        
        ClearConnections();
    }
    
    private void OnDestroy()
    {
        ClearMinimap();
    }
}

