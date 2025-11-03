using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool isPlayerAlive = true;
    public bool isPaused = false;
    public GameObject InGamePauseMenu;
    public GameObject InGameEndingMenu;
    public GameObject InGameWinMenu;
    public GameObject TutorialText;
    public PlayerControllerTest player;
    public PlayerGhost ghost;
    public EnemySpawner enemySpawner;
    [Header("Dungeon Rooms")]
    // Rooms in dungeon order. Assign in the inspector or dynamically at runtime.
    public RoomManager[] rooms;
    private int currentRoomIndex = -1;
    public bool isInLevel;
    public float levelStartTime;
    
    public TMP_Text infoText;
    public TMP_Text displayScoreText;
    // Google analytics temporarily disabled: keep field commented for now
    // private SendToGoogle sendToGoogle;
    //delegate for reset states
    public delegate void OnResetDelegate();
    public event OnResetDelegate onReset;
    public int CurrentWave => waveCount;
    /*
        void OnEnable()
        {
            InputSystem.actions["Reset"].performed += OnReset;
        }

        void OnDisable()
        {
            InputSystem.actions["Reset"].performed -= OnReset;

        }
        */

    //private void OnReset(InputAction.CallbackContext ctx) => ResetWithGhost();

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
        if (isInLevel)
        {
            levelStartTime = Time.time;
            InGamePauseMenu.SetActive(false);
            InGameEndingMenu.SetActive(false);
            InGameWinMenu.SetActive(false);
            PlayerControllerTest pcTest = FindFirstObjectByType<PlayerControllerTest>();
            pcTest.playerInput.Default.Escape.performed += OnEscapeTriggered;
            // sendToGoogle = FindFirstObjectByType<SendToGoogle>(); // disabled
        }
        Time.timeScale = 0.0f;
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
        currentRoomIndex = 0;
        // Refresh the room progress UI
        UpdateRoomProgressUI();
    }

    // Called by RoomManager when a room is cleared
    public void RoomCleared(RoomManager room)
    {
        // Only advance if the cleared room matches the current room (defensive)
        if (currentRoomIndex >= 0 && currentRoomIndex < rooms.Length)
        {
            Debug.Log($"GameManager: Room '{room.name}' cleared. Advancing to room {currentRoomIndex + 1}");
            currentRoomIndex++;
            UpdateRoomProgressUI();
        }
    }

    // Display current room name in red followed by cleared/total, e.g.:
    // <color=red>Room_2</color>: 1/5
    private void UpdateRoomProgressUI()
    {
        if (infoText == null) return;
        if (rooms == null || rooms.Length == 0)
        {
            infoText.text = "Rooms: 0/0";
            return;
        }

        int clearedCount = Mathf.Clamp(currentRoomIndex, 0, rooms.Length);
        infoText.text = $"<color=#FF0000>Rooms</color>: {clearedCount}/{rooms.Length}";
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

    // Wave lifecycle is now handled by RoomManager and EnemySpawner.
    
    public void GameOver()
    {
        //player.gameObject.SetActive(false);
        isPlayerAlive = false;
        Time.timeScale = 0;
        InGameEndingMenu.SetActive(true);
        // Show survival time instead of wave count (waves managed by rooms)
        float totalSurvivalTime = Time.time - levelStartTime;
        displayScoreText.text = $"<size=20><color=#FF0000>Time Survived: </color>{totalSurvivalTime:F1}s</size>";

    // Google analytics disabled in GameOver
    }
    // Main Menu button's functions
    public void NewGame()
    {
        SceneManager.LoadScene("AlphaProgressCheck");
    }
    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; 
        #else
            Application.Quit(); 
        #endif
    }


    //InGameMenu button's functions

    public void Resume()
    {
        ChangePauseStat();
    }
    public void Restart()
    { 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        //InitializePauseStat();
    }

    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    //helper functions
    private void ChangePauseStat()
    {
        isPaused = !isPaused;
        if (isPaused)
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
    private void InitializePauseStat()
    {
        //isPaused = false;
        Time.timeScale = 1;
    }
    private void OnEscapeTriggered(InputAction.CallbackContext Context)
    {
        if (isPlayerAlive)
            ChangePauseStat();
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

        // Google analytics disabled in ShowWinMenu
        // if (sendToGoogle != null)
        // {
        //     float totalSurvivalTime = Time.time - levelStartTime;
        //     int finalWaveCount = waveCount - 1; // Assuming waveCount is the next wave, so completed waves are waveCount - 1
        //     sendToGoogle.SendGameSummary(totalSurvivalTime, finalWaveCount);
        // }
    }

    //public void ResetWithGhost()
    //{
    //    List<ObjectState> playerStates = new List<ObjectState>(player.sendStates());
    //    Reset();
    //    GhostController newGhost = Instantiate(ghost);
    //    newGhost.InitializeGhost(player.initialPosition, playerStates);

    //}

    public void Reset()
    {
        onReset?.Invoke();
        // Destroy all enemy spawn indicators to prevent spawning during reset
        //EnemySpawnIndicator[] indicators = FindObjectsByType<EnemySpawnIndicator>(FindObjectsInactive.Include, FindObjectsSortMode.None); 
        //foreach (EnemySpawnIndicator i in indicators)
        //{
        //        Destroy(i.gameObject);
        //}

        Bullet_Default[] bullets = FindObjectsByType<Bullet_Default>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Bullet_Default b in bullets)
        {
            Destroy(b.gameObject);
        }

        // Reset no longer directly triggers enemy spawner; RoomManager/EnemySpawner
        // will handle spawning for their rooms. If needed, individual spawners
        // can be reset via their own APIs or via the onReset delegate listeners.

    }
}
