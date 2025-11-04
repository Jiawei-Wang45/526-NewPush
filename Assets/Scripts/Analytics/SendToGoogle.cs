using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SendToGoogle : MonoBehaviour
{
    [SerializeField] private string TimerURL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLSeQYt1oz7owR6m0aq2l06AjYu39TZzs6AoKogqTJiN2Jzv_EQ/formResponse";
    [SerializeField] private string AbilityUsageURL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLSd_BBrcB4RPYBtHgK43YJNURHGRGxHtWYOS2kl0m9XhNQVinw/formResponse";
    [SerializeField] private string AbilityURL = "https://docs.google.com/forms/u/2/d/e/1FAIpQLSepImIACWgwQG4mlqo907E4wAC4d5Pe7WxYqOqoW4JfEmSXnQ/formResponse";

    private string _sessionID;
    private const string FIELD_SESSION = "entry.177888417"; // Session ID
    private const string FIELD_TIME = "entry.1373821541"; // Time
    private const string FIELD_PASSED = "entry.1713529862"; // Passed (boolean)
    private const string FIELD_ROOM_NUMBER = "entry.686191148"; // Room number
    private const string FIELD_WEAPON_COUNT = "entry.652169316"; // Weapon count
    private const string FIELD_ATTACKING_ABILITIES_COUNT = "entry.108973933"; // Attacking abilities count
    private const string FIELD_DEFENSE_ABILITIES_COUNT = "entry.676975950"; // Defense abilities count
    private const string FIELD_SURVIVE_TIME = "entry.1373821541"; // Survive time
    private const string FIELD_WIN = "entry.1713529862"; // Win (boolean)
    private const string FIELD_WEAPON_TYPE = "entry.652169316"; // Weapon type
    private const string FIELD_ATTACKING_ABILITIES = "entry.108973933"; // Attacking abilities
    private const string FIELD_DEFENSE_ABILITIES = "entry.676975950"; // Defense abilities

    private float _startTime;
    [Header("Networking")]
    [SerializeField] private int requestTimeoutSeconds = 10;

    private void Awake()
    {
        // Assign sessionID to identify playtests
        string prefix = "";
#if UNITY_EDITOR
        prefix = "LOCAL_";
#elif UNITY_WEBGL
        // Check if running on GitHub Pages or similar web hosting
        if (Application.absoluteURL.Contains("github.io") || Application.absoluteURL.Contains("pages.github.com"))
        {
            prefix = "WEB_";
        }
        else
        {
            prefix = "WEB_LOCAL_";
        }
#else
        prefix = "BUILD_";
#endif
        _sessionID = prefix + DateTime.Now.Ticks;
        _startTime = Time.realtimeSinceStartup;
        Debug.Log($"[SendToGoogle] Awake - sessionID={_sessionID} startTime={_startTime} URL={Application.absoluteURL}");
    }

    // Send timer data
    public void SendTimerData(float roomTime, bool passed, int roomNumber)
    {
        string session = _sessionID;
        string sessionStr = session;
        string timeStr = roomTime.ToString("F3");
        string passedStr = passed.ToString().ToLower();
        string roomStr = roomNumber.ToString();

        Debug.Log($"[SendToGoogle] SendTimerData -> session:{sessionStr} time:{timeStr} passed:{passedStr} room:{roomStr}");
        StartCoroutine(PostTimer(sessionStr, timeStr, passedStr, roomStr));
    }

    // Send ability usage data
    public void SendAbilityUsageData(int weaponCount, int attackingAbilitiesCount, int defenseAbilitiesCount, float surviveTime, bool win, int roomNumber)
    {
        string session = _sessionID;
        string sessionStr = session;
        string weaponCountStr = weaponCount.ToString();
        string attackingCountStr = attackingAbilitiesCount.ToString();
        string defenseCountStr = defenseAbilitiesCount.ToString();
        string surviveTimeStr = surviveTime.ToString("F3");
        string winStr = win.ToString().ToLower();
        string roomStr = roomNumber.ToString();

        Debug.Log($"[SendToGoogle] SendAbilityUsageData -> session:{sessionStr} weaponCount:{weaponCountStr} attacking:{attackingCountStr} defense:{defenseCountStr} surviveTime:{surviveTimeStr} win:{winStr} room:{roomStr}");
        StartCoroutine(PostAbilityUsage(sessionStr, weaponCountStr, attackingCountStr, defenseCountStr, surviveTimeStr, winStr, roomStr));
    }

    // Send ability data
    public void SendAbilityData(string weaponType, string attackingAbilities, string defenseAbilities)
    {
        string session = _sessionID;
        string sessionStr = session;

        Debug.Log($"[SendToGoogle] SendAbilityData -> session:{sessionStr} weaponType:{weaponType} attacking:{attackingAbilities} defense:{defenseAbilities}");
        StartCoroutine(PostAbility(sessionStr, weaponType, attackingAbilities, defenseAbilities));
    }

    private IEnumerator PostTimer(string sessionID, string time, string passed, string roomNumber)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_TIME, time);
        form.AddField(FIELD_PASSED, passed);
        form.AddField(FIELD_ROOM_NUMBER, roomNumber);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting timer to {TimerURL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_TIME}={time}, {FIELD_PASSED}={passed}, {FIELD_ROOM_NUMBER}={roomNumber}");

        using (UnityWebRequest www = UnityWebRequest.Post(TimerURL, form))
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
                Debug.LogError($"[SendToGoogle] Timer post failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Timer form upload complete! statusCode: {www.responseCode}");
            }
        }
    }

    private IEnumerator PostAbilityUsage(string sessionID, string weaponCount, string attackingCount, string defenseCount, string surviveTime, string win, string roomNumber)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_WEAPON_COUNT, weaponCount);
        form.AddField(FIELD_ATTACKING_ABILITIES_COUNT, attackingCount);
        form.AddField(FIELD_DEFENSE_ABILITIES_COUNT, defenseCount);
        form.AddField(FIELD_SURVIVE_TIME, surviveTime);
        form.AddField(FIELD_WIN, win);
        form.AddField(FIELD_ROOM_NUMBER, roomNumber);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting ability usage to {AbilityUsageURL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_WEAPON_COUNT}={weaponCount}, {FIELD_ATTACKING_ABILITIES_COUNT}={attackingCount}, {FIELD_DEFENSE_ABILITIES_COUNT}={defenseCount}, {FIELD_SURVIVE_TIME}={surviveTime}, {FIELD_WIN}={win}, {FIELD_ROOM_NUMBER}={roomNumber}");

        using (UnityWebRequest www = UnityWebRequest.Post(AbilityUsageURL, form))
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
                Debug.LogError($"[SendToGoogle] Ability usage post failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Ability usage form upload complete! statusCode: {www.responseCode}");
            }
        }
    }

    private IEnumerator PostAbility(string sessionID, string weaponType, string attackingAbilities, string defenseAbilities)
    {
        WWWForm form = new WWWForm();

        form.AddField(FIELD_SESSION, sessionID);
        form.AddField(FIELD_WEAPON_TYPE, weaponType);
        form.AddField(FIELD_ATTACKING_ABILITIES, attackingAbilities);
        form.AddField(FIELD_DEFENSE_ABILITIES, defenseAbilities);

        // Log form contents
        Debug.Log($"[SendToGoogle] Posting ability to {AbilityURL} with fields: {FIELD_SESSION}={sessionID}, {FIELD_WEAPON_TYPE}={weaponType}, {FIELD_ATTACKING_ABILITIES}={attackingAbilities}, {FIELD_DEFENSE_ABILITIES}={defenseAbilities}");

        using (UnityWebRequest www = UnityWebRequest.Post(AbilityURL, form))
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
                Debug.LogError($"[SendToGoogle] Ability post failed: {www.error}  statusCode: {www.responseCode}");
                // also log response text if any (may be empty)
                string resp = www.downloadHandler != null ? www.downloadHandler.text : "<no-downloadHandler>";
                Debug.LogWarning($"[SendToGoogle] response body: {resp}");
            }
            else
            {
                Debug.Log($"[SendToGoogle] Ability form upload complete! statusCode: {www.responseCode}");
            }
        }
    }
}
