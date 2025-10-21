using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendToGoogle : MonoBehaviour
{
    [SerializeField] private string URL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLSdMv2OjR-wEv6eOubeQ4n7LzH69-5BVpZs1KIMNYE3fg0vl1w/formResponse";
    [SerializeField] private string WaveURL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLScablE3Fmg8MNJkDhF9fbZDj89CHzYVwf_-R3rmSS9B7a0Fgg/formResponse";
    [SerializeField] private string GameSummaryURL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLSeW9pjkvfOi6P7DYz_Q80-lzYAGcoV1tygJCieJoyRDpaES4Q/formResponse";

    private long _sessionID;
    private const string FIELD_SESSION = "entry.222600633"; // Session
    private const string FIELD_TIME = "entry.2115569534"; // Time since start to ability use
    private const string FIELD_WAVE = "entry.865303843"; // wave number
    private const string FIELD_POSITION = "entry.1199288425"; // player position x,y
    private const string FIELD_WAVE_DURATION = "entry.2115569534"; // Wave duration
    private const string FIELD_WAVE_NUMBER = "entry.865303843"; // Wave number
    private const string FIELD_TOTAL_SURVIVAL_TIME = "entry.2115569534"; // Total survival time
    private const string FIELD_FINAL_WAVE_COUNT = "entry.865303843"; // Final wave count
       [SerializeField] private int currentWave = 0;

    private float _startTime;
    [Header("Networking")]
    [SerializeField] private int requestTimeoutSeconds = 10;

    private void Awake()
    {
        // Assign sessionID to identify playtests
        _sessionID = DateTime.Now.Ticks;
        _startTime = Time.realtimeSinceStartup;
        Debug.Log($"[SendToGoogle] Awake - sessionID={_sessionID} startTime={_startTime} URL={URL}");
    }
    // Send ability usage with concrete tracked fields
    public void SendAbilityUse(Vector2 playerPosition, int waveOverride = -1)
    {
        long session = _sessionID;
        float timeSinceStart = Time.realtimeSinceStartup - _startTime;
        int waveToSend = waveOverride >= 0 ? waveOverride : currentWave;

        string sessionStr = session.ToString();
        string timeStr = timeSinceStart.ToString("F3");
        string waveStr = waveToSend.ToString();
        string posStr = playerPosition.x.ToString("F3") + "," + playerPosition.y.ToString("F3");

        Debug.Log($"[SendToGoogle] SendAbilityUse -> session:{sessionStr} time:{timeStr} wave:{waveStr} position:{posStr}");
        StartCoroutine(PostAbility(sessionStr, timeStr, waveStr, posStr));
    }

    // Send wave data
    public void SendWaveData(float waveDuration, int waveNumber)
    {
        long session = _sessionID;
        string sessionStr = session.ToString();
        string durationStr = waveDuration.ToString("F3");
        string waveStr = waveNumber.ToString();

        Debug.Log($"[SendToGoogle] SendWaveData -> session:{sessionStr} duration:{durationStr} wave:{waveStr}");
        StartCoroutine(PostWave(sessionStr, durationStr, waveStr));
    }

    // Send game summary data
    public void SendGameSummary(float totalSurvivalTime, int finalWaveCount)
    {
        long session = _sessionID;
        string sessionStr = session.ToString();
        string timeStr = totalSurvivalTime.ToString("F3");
        string waveStr = finalWaveCount.ToString();

        Debug.Log($"[SendToGoogle] SendGameSummary -> session:{sessionStr} totalTime:{timeStr} finalWave:{waveStr}");
        StartCoroutine(PostGameSummary(sessionStr, timeStr, waveStr));
    }

    private IEnumerator PostAbility(string sessionID, string time, string wave, string position)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_TIME, time);
        form.AddField(FIELD_WAVE, wave);
        form.AddField(FIELD_POSITION, position);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting to {URL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_TIME}={time}, {FIELD_WAVE}={wave}, {FIELD_POSITION}={position}");

        using (UnityWebRequest www = UnityWebRequest.Post(URL, form))
        {
            // set timeout (Unity 2020.1+ supports timeout property)
            try { www.timeout = requestTimeoutSeconds; } catch { }
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"[SendToGoogle] failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Form upload complete! statusCode: {www.responseCode}");
            }
        }
    }

    private IEnumerator PostWave(string sessionID, string duration, string wave)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_WAVE_DURATION, duration);
        form.AddField(FIELD_WAVE_NUMBER, wave);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting wave to {WaveURL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_WAVE_DURATION}={duration}, {FIELD_WAVE_NUMBER}={wave}");

        using (UnityWebRequest www = UnityWebRequest.Post(WaveURL, form))
        {
            // set timeout (Unity 2020.1+ supports timeout property)
            try { www.timeout = requestTimeoutSeconds; } catch { }
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"[SendToGoogle] Wave post failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Wave form upload complete! statusCode: {www.responseCode}");
            }
        }
    }

    private IEnumerator PostGameSummary(string sessionID, string totalTime, string finalWave)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_TOTAL_SURVIVAL_TIME, totalTime);
        form.AddField(FIELD_FINAL_WAVE_COUNT, finalWave);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting game summary to {GameSummaryURL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_TOTAL_SURVIVAL_TIME}={totalTime}, {FIELD_FINAL_WAVE_COUNT}={finalWave}");

        using (UnityWebRequest www = UnityWebRequest.Post(GameSummaryURL, form))
        {
            // set timeout (Unity 2020.1+ supports timeout property)
            try { www.timeout = requestTimeoutSeconds; } catch { }
            yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (www.result != UnityWebRequest.Result.Success)
#else
            if (www.isNetworkError || www.isHttpError)
#endif
            {
                Debug.LogError($"[SendToGoogle] Game summary post failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Game summary form upload complete! statusCode: {www.responseCode}");
            }
        }
    }
}
