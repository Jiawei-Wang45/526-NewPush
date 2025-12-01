using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    //components and external reference
    [NonSerialized] private PlayerController pc;

    [Header("UI paramaters")]
    public bool isPlayerAlive = true;
    public bool IsPaused { get; set; } = false;
    public TMP_Text AmmoCounter;
    public GameObject InGamePauseMenu;
    public GameObject InGameEndingMenu;
    public GameObject InGameWinMenu;
    public DungeonRoomInfo InGameDungeonRoomInfo;
    public SurvivalTime InGameSurvivalTime;
    public DungeonMinimap InGameDungeonMinimap;
    public GameObject TutorialText;
    public bool isInLevel; //in contrast with tutorial level

    [Header("Level Selection")]
    // Level scenes array, matching GameManagerForMenu for consistency
    public string[] levelScenes = { "Level_1", "Level_2", "Level_3" };

    [Header("Dungeon Rooms")]
    // Rooms in dungeon order. Assign in the inspector or dynamically at runtime.
    public RoomManager[] rooms;
    // Number of rooms that have been cleared (not necessarily in order)
    public int clearedRoomCount = 0;
    // Array tracking which rooms have been cleared (indexed by room array index)
    private bool[] roomClearedStatus;
    public float levelStartTime;
    private float roomStartTime;
    public int selectedLevel = 1; // Selected level for analytics
    
    // Graph structure from dungeon generator (for minimap and navigation)
    // Maps room index to list of connected room indices
    public Dictionary<int, List<int>> roomAdjacencyGraph = new Dictionary<int, List<int>>();
    // Grid cell positions for each room (indexed by room array index)
    public List<Vector2Int> roomGridPositions = new List<Vector2Int>();
    // Cell size used for grid layout (for converting between grid and world positions)
    public Vector2 dungeonCellSize = Vector2.zero;
    // Start and end room indices from dungeon generator
    public int startRoomIndex = -1;
    public int endRoomIndex = -1;
    
    
    //Google analytics
    private SendToGoogle sendToGoogle;
    // Ability usage counters
    private int weaponUseCount = 0;
    private int attackingAbilitiesUseCount = 0;
    private int defenseAbilitiesUseCount = 0;
    // Analytics sent flags to prevent duplicate sends
    private bool hasSentAbilityData = false;
    private bool hasSentAbilityUsage = false;
    private int lastSentTimerRoom = -1;

    ////delegate for reset states
    //public delegate void OnResetDelegate();
    //public event OnResetDelegate onReset;

    #region analytics
    // Methods to increment counters
    public void IncrementWeaponUseCount()
    {
        weaponUseCount++;
    }

    public void IncrementAttackingAbilitiesUseCount()
    {
        attackingAbilitiesUseCount++;
    }

    public void IncrementDefenseAbilitiesUseCount()
    {
        defenseAbilitiesUseCount++;
    }

    // Send ability data (weapon and ability names)
    private void SendAbilityData()
    {
        if (sendToGoogle != null && !hasSentAbilityData)
        {
            string weaponType = CharacterConfigHolder.instance != null && CharacterConfigHolder.instance.weapon != null ? CharacterConfigHolder.instance.weapon.name : "Unknown";
            string attackingAbilities = CharacterConfigHolder.instance != null && CharacterConfigHolder.instance.attackingAbility != null ? CharacterConfigHolder.instance.attackingAbility.abilityName : "Unknown";
            string defenseAbilities = CharacterConfigHolder.instance != null && CharacterConfigHolder.instance.defenseAbility != null ? CharacterConfigHolder.instance.defenseAbility.abilityName : "Unknown";

            sendToGoogle.SendAbilityData(weaponType, attackingAbilities, defenseAbilities, selectedLevel);
            hasSentAbilityData = true;
        }
    }
    public void GameOver()
    {
        //player.gameObject.SetActive(false);
        isPlayerAlive = false;
        Time.timeScale = 0;
        InGameEndingMenu.SetActive(true);
        // Show survival time instead of wave count (waves managed by rooms)
        float totalSurvivalTime = Time.time - levelStartTime;
        InGameSurvivalTime.UpdateSurvivalTime(totalSurvivalTime);

        // Send data only for rooms 1 and above (skip room0)
        int roomNumber = clearedRoomCount;
        if (roomNumber >= 1)
        {
            // Send timer data for the current room (death) only if not already sent
            if (sendToGoogle != null && lastSentTimerRoom != roomNumber)
            {
                float roomTime = Time.time - roomStartTime;
                sendToGoogle.SendTimerData(roomTime, false, roomNumber, selectedLevel);
                lastSentTimerRoom = roomNumber;
            }

            // Send ability usage data on game over (death) only once
            if (sendToGoogle != null && !hasSentAbilityUsage)
            {
                sendToGoogle.SendAbilityUsageData(weaponUseCount, attackingAbilitiesUseCount, defenseAbilitiesUseCount, totalSurvivalTime, false, roomNumber, selectedLevel);
                hasSentAbilityUsage = true;
            }
        }
    }
    #endregion analytics


    #region initialization
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
    private void Start()
    {
        pc= FindFirstObjectByType<PlayerController>();
        
        InGamePauseMenu.SetActive(false);
        InGameEndingMenu.SetActive(false);
        InGameWinMenu.SetActive(false);
        if (isInLevel)
        {
            levelStartTime = Time.time;
            pc.playerInput.Default.Escape.performed += OnEscapeTriggered;
            sendToGoogle = FindFirstObjectByType<SendToGoogle>();
            // Set selected level from config
            selectedLevel = CharacterConfigHolder.instance.selectedLevelIndex + 1;
            // Start coroutine to update minimap when player enters rooms
            StartCoroutine(UpdateMinimapOnRoomEnter());
            // Send ability data at the start of the level
            SendAbilityData();
        }
        else
        {
            TutorialText.SetActive(false);
        }
        InitializePauseStat();
    }
    
    // Coroutine to periodically check if player entered a new room and update minimap
    private IEnumerator UpdateMinimapOnRoomEnter()
    {
        int lastRoomIndex = -1;
        while (isPlayerAlive)
        {
            int currentRoomIndex = GetCurrentRoomIndex();
            if (currentRoomIndex != lastRoomIndex)
            {
                lastRoomIndex = currentRoomIndex;
                UpdateMinimap();
            }
            yield return new WaitForSeconds(0.5f); // Check every 0.5 seconds
        }
    }
    //private void Update()
    //{
    //    if (isInLevel && isPlayerAlive &&  Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        ChangePauseStat();
    //    }
    //}

    public void GameStart()
    {
        TutorialText.SetActive(false);
        Time.timeScale = 1.0f;
        // Reset analytics flags for new game
        hasSentAbilityData = false;
        hasSentAbilityUsage = false;
        lastSentTimerRoom = -1;
        // Reset counters
        weaponUseCount = 0;
        attackingAbilitiesUseCount = 0;
        defenseAbilitiesUseCount = 0;
        // Start dungeon flow: activate first room (if any)
        StartDungeon();
    }

    // Start dungeon progression from the first room
    public void StartDungeon()
    {
        if (rooms == null || rooms.Length == 0)
        {
            // No rooms configured: nothing to start here. RoomManager/EnemySpawner
            // should manage enemy spawning per-room. Simply return.
            return;
        }
        clearedRoomCount = 0;
        // Initialize room cleared status array
        roomClearedStatus = new bool[rooms.Length];
        for (int i = 0; i < roomClearedStatus.Length; i++)
        {
            roomClearedStatus[i] = false;
        }
        roomStartTime = Time.time;
        // Refresh the room progress UI
        UpdateRoomProgressUI();

        // Send ability data at the start of the game
        SendAbilityData();
    }
    
    /// <summary>
    /// Set the dungeon graph structure (called by ProceduralDungeonGenerator after generation)
    /// </summary>
    public void SetDungeonGraph(Dictionary<int, List<int>> adjacencyGraph, List<Vector2Int> gridPositions, Vector2 cellSize, int startIndex = -1, int endIndex = -1)
    {
        roomAdjacencyGraph = adjacencyGraph ?? new Dictionary<int, List<int>>();
        roomGridPositions = gridPositions ?? new List<Vector2Int>();
        dungeonCellSize = cellSize;
        startRoomIndex = startIndex;
        endRoomIndex = endIndex;
    }
    
    /// <summary>
    /// Get connected room indices for a given room index
    /// </summary>
    public List<int> GetConnectedRooms(int roomIndex)
    {
        if (roomAdjacencyGraph != null && roomAdjacencyGraph.TryGetValue(roomIndex, out var connected))
        {
            return connected;
        }
        return new List<int>();
    }
    #endregion initialization

    #region dungeon update
    // Called by RoomManager when a room is cleared
    public void RoomCleared(RoomManager room)
    {
        // Ensure roomClearedStatus array is initialized
        if (roomClearedStatus == null || rooms == null || rooms.Length == 0)
        {
            Debug.LogWarning("GameManager: Room cleared status array not initialized. Initializing now...");
            if (rooms != null && rooms.Length > 0)
            {
                roomClearedStatus = new bool[rooms.Length];
                for (int i = 0; i < roomClearedStatus.Length; i++)
                {
                    roomClearedStatus[i] = false;
                }
                clearedRoomCount = 0;
            }
            else
            {
                Debug.LogError("GameManager: Cannot initialize room cleared status - rooms array is null or empty!");
                return;
            }
        }
        
        // Find the index of the cleared room
        int roomIndex = GetRoomIndex(room);
        if (roomIndex < 0 || roomIndex >= rooms.Length)
        {
            Debug.LogWarning($"GameManager: Room '{room.name}' not found in rooms array!");
            return;
        }

        // Check if this room was already cleared (prevent duplicate clearing)
        if (roomClearedStatus[roomIndex])
        {
            Debug.Log($"GameManager: Room '{room.name}' was already cleared, ignoring duplicate clear.");
            return;
        }

        // Mark room as cleared
        roomClearedStatus[roomIndex] = true;
        clearedRoomCount++;
        
        float roomTime = Time.time - roomStartTime;

        // Send timer data only for rooms 1 and above (skip room0)
        if (sendToGoogle != null && clearedRoomCount >= 1 && lastSentTimerRoom != clearedRoomCount)
        {
            sendToGoogle.SendTimerData(roomTime, true, clearedRoomCount, selectedLevel);
            lastSentTimerRoom = clearedRoomCount;
        }

        Debug.Log($"GameManager: Room '{room.name}' (index {roomIndex}) cleared. Total cleared: {clearedRoomCount}/{rooms.Length}");
        roomStartTime = Time.time; // Reset for next room
        UpdateRoomProgressUI();
        UpdateMinimap();
    }
    
    /// <summary>
    /// Get the index of a room in the rooms array
    /// </summary>
    private int GetRoomIndex(RoomManager room)
    {
        if (rooms == null || room == null) return -1;
        
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] == room)
            {
                return i;
            }
        }
        return -1;
    }
    
    /// <summary>
    /// Check if a room has been cleared
    /// </summary>
    public bool IsRoomCleared(int roomIndex)
    {
        // If array is not initialized, initialize it
        if (roomClearedStatus == null)
        {
            if (rooms != null && rooms.Length > 0)
            {
                roomClearedStatus = new bool[rooms.Length];
                for (int i = 0; i < roomClearedStatus.Length; i++)
                {
                    roomClearedStatus[i] = false;
                }
            }
            else
            {
                return false;
            }
        }
        
        if (roomIndex < 0 || roomIndex >= roomClearedStatus.Length)
        {
            return false;
        }
        return roomClearedStatus[roomIndex];
    }
    
    /// <summary>
    /// Check if a room has been cleared by RoomManager reference
    /// </summary>
    public bool IsRoomCleared(RoomManager room)
    {
        int index = GetRoomIndex(room);
        return IsRoomCleared(index);
    }
    
    /// <summary>
    /// Get the room index where the player is currently located (if any)
    /// Returns -1 if player is not in any room
    /// </summary>
    public int GetCurrentRoomIndex()
    {
        if (pc == null || rooms == null) return -1;
        
        Vector3 playerPos = pc.transform.position;
        for (int i = 0; i < rooms.Length; i++)
        {
            if (rooms[i] != null && rooms[i].roomTrigger != null)
            {
                if (rooms[i].roomTrigger.bounds.Contains(playerPos))
                {
                    return i;
                }
            }
        }
        return -1;
    }
    #endregion dungeon update

    #region UI update call

    // Display current room name in red followed by cleared/total, e.g.:
    // <color=red>Room_2</color>: 1/5
    private void UpdateRoomProgressUI()
    {
        int clearedCount = Mathf.Clamp(clearedRoomCount, 0, rooms.Length);
        InGameDungeonRoomInfo.UpdateRoomInfo(clearedCount, rooms.Length);
    }
    
    // Update the dungeon minimap UI
    private void UpdateMinimap()
    {
        if (InGameDungeonMinimap != null)
        {
            InGameDungeonMinimap.RefreshMinimap();
        }
    }

    public void UpdateAmmo(int ammo, int maxAmmo)
    {
        AmmoCounter.text = $"{ammo}/{maxAmmo}";
    }

    // Called by a WinTrigger when the player reaches the goal
    public void PlayerReachedWinTrigger()
    {
        ShowWinMenu();
    }

    public void PlayerDestroyed()
    {
        // No more resets: when player is destroyed, immediately end the game
        GameOver();
    }
    public void ShowWinMenu()
    {
        isPlayerAlive = false;
        Time.timeScale = 0;
        InGameWinMenu.SetActive(true);

        // Record level completion time
        float completionTime = Time.time - levelStartTime;
        string levelName = SceneManager.GetActiveScene().name;
        GameAnalyticsManager gaManager = FindFirstObjectByType<GameAnalyticsManager>();
        if (gaManager != null)
        {
            gaManager.SendLevelCompletedEvent(levelName, completionTime);
        }

        // Send data only for rooms 1 and above (skip room0)
        int roomNumber = clearedRoomCount;
        if (roomNumber >= 1)
        {
            // Send ability usage data on win only once
            if (sendToGoogle != null && !hasSentAbilityUsage)
            {
                sendToGoogle.SendAbilityUsageData(weaponUseCount, attackingAbilitiesUseCount, defenseAbilitiesUseCount, completionTime, true, roomNumber, selectedLevel);
                hasSentAbilityUsage = true;
            }
        }
    }
    // Wave lifecycle is now handled by RoomManager and EnemySpawner.

    #endregion UI update call

    #region MainMenu button callback
    //public void NewGame()
    //{
    //    SceneManager.LoadScene("AlphaProgressCheck");
    //}
    //public void Exit()
    //{
    //    #if UNITY_EDITOR
    //        UnityEditor.EditorApplication.isPlaying = false; 
    //    #else
    //        Application.Quit(); 
    //    #endif
    //}

    #endregion MainMenu button callback

    #region AlphaProgressCheck button callback
    //InGameMenu button's functions

    public void Resume()
    {
        ChangePauseStat();
    }
    public void Restart()
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Load the next level scene. Can be called by UI buttons.
    /// References GameManagerForMenu's levelScenes array approach.
    /// Finds current scene in levelScenes array and loads the next one.
    /// If no next level exists, returns to MainMenu.
    /// </summary>
    public void LoadNextLevel()
    {
        if (levelScenes == null || levelScenes.Length == 0)
        {
            Debug.LogWarning("[GameManager] levelScenes array is not configured. Returning to MainMenu.");
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // Find current scene index in levelScenes array
        int currentLevelIndex = -1;
        for (int i = 0; i < levelScenes.Length; i++)
        {
            if (levelScenes[i] == currentSceneName)
            {
                currentLevelIndex = i;
                break;
            }
        }

        // If current scene is found in array, load next level
        if (currentLevelIndex >= 0)
        {
            int nextLevelIndex = currentLevelIndex + 1;
            
            // Check if next level exists
            if (nextLevelIndex < levelScenes.Length && !string.IsNullOrEmpty(levelScenes[nextLevelIndex]))
            {
                Time.timeScale = 1; // Reset time scale before loading
                SceneManager.LoadScene(levelScenes[nextLevelIndex]);
                return;
            }
            else
            {
                Debug.Log($"[GameManager] No next level available (current: {currentLevelIndex}, total: {levelScenes.Length}). Returning to MainMenu.");
            }
        }
        else
        {
            Debug.LogWarning($"[GameManager] Current scene '{currentSceneName}' not found in levelScenes array. Returning to MainMenu.");
        }
        
        // Fallback: return to main menu
        Time.timeScale = 1; // Reset time scale before loading
        SceneManager.LoadScene("MainMenu");
    }
    //helper functions
    private void ChangePauseStat()
    {
        IsPaused = !IsPaused;
        if (IsPaused)
        {
            Time.timeScale = 0;
            InGamePauseMenu.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            InGamePauseMenu.SetActive(false);
        }

    }
    
    private void OnEscapeTriggered(InputAction.CallbackContext Context)
    {
        if (isPlayerAlive)
            ChangePauseStat();
    }

    private void InitializePauseStat()
    {
        IsPaused = false;
        Time.timeScale = 1;
    }
    #endregion AlphaProgressCheck button callback

    //private void InitializePauseStat()
    //{
    //    //isPaused = false;
    //    Time.timeScale = 1;
    //}

    //public void Reset()
    //{
    //    onReset?.Invoke();
    //    // Reset analytics flags and counters
    //    hasSentAbilityData = false;
    //    hasSentAbilityUsage = false;
    //    lastSentTimerRoom = -1;
    //    weaponUseCount = 0;
    //    attackingAbilitiesUseCount = 0;
    //    defenseAbilitiesUseCount = 0;
    //    // Destroy all enemy spawn indicators to prevent spawning during reset
    //    //EnemySpawnIndicator[] indicators = FindObjectsByType<EnemySpawnIndicator>(FindObjectsInactive.Include, FindObjectsSortMode.None); 
    //    //foreach (EnemySpawnIndicator i in indicators)
    //    //{
    //    //        Destroy(i.gameObject);
    //    //}

    //    Bullet_Default[] bullets = FindObjectsByType<Bullet_Default>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    //    foreach (Bullet_Default b in bullets)
    //    {
    //        Destroy(b.gameObject);
    //    }

    //    // Reset no longer directly triggers enemy spawner; RoomManager/EnemySpawner
    //    // will handle spawning for their rooms. If needed, individual spawners
    //    // can be reset via their own APIs or via the onReset delegate listeners.

    //}
}
