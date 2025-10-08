using UnityEngine;
using GameAnalyticsSDK;
public class GameAnalyticsManager : MonoBehaviour
{
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
                GameAnalytics.Initialize();
                Debug.Log("Game Analytics Initialized");
        }

        public void SendLevelCompletedEvent(string levelName, float completionTime)
    {
      
        GameAnalytics.NewDesignEvent($"LevelCompleted:{levelName}", completionTime);
        Debug.Log($"GA Event: Level {levelName} Completed in {completionTime} seconds");
    }
}
