using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public bool isPlayerAlive = true;
    public bool isPaused = false;
    public GameObject InGamePauseMenu;
    public GameObject InGameEndingMenu;
    public GameObject InGameWinMenu;
    public PlayerControllerTest player;
    public GhostController ghost;
    public bool isInLevel;
    public float levelStartTime;

        void OnEnable()
    {
        InputSystem.actions["Reset"].performed += OnReset;
    }

    void OnDisable()
    {
        InputSystem.actions["Reset"].performed -= OnReset;

    }

    private void OnReset(InputAction.CallbackContext ctx) => ResetWithGhost();


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
        }
        Time.timeScale = 1.0f;
    }
    //private void Update()
    //{
    //    if (isInLevel && isPlayerAlive &&  Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        ChangePauseStat();
    //    }
    //}

    public void PlayerDead()
    {
        isPlayerAlive = false;
        Time.timeScale = 0;
        InGameEndingMenu.SetActive(true);

    }
    // Main Menu button's functions
    public void NewGame()
    {
        SceneManager.LoadScene("Level_0");
        //InitializePauseStat();
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
    }

    public void ResetWithGhost()
    {
        List<ObjectState> playerStates = new List<ObjectState>(player.sendStates());
        Reset();
        GhostController newGhost = Instantiate(ghost);
        newGhost.InitializeGhost(player.initialPosition, playerStates);

    }

    public void Reset()
    {

        player.Reset();

        GhostController[] ghosts = FindObjectsByType<GhostController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GhostController g in ghosts)
        {
            g.Reset();
        }
    
        /*
        EnemyController[] enemyObjects = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (EnemyController e in enemyObjects)
        {
            e.Reset();
        }
        */

        Bullet_Default[] bullets = FindObjectsByType<Bullet_Default>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Bullet_Default b in bullets)
        {
            Destroy(b.gameObject);
        }

    }
}
