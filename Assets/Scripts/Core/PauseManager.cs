using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    //components
    public static PauseManager instance;

    //pause variables
    public bool isPausing = false;
    public float activePauseStrength = 1.0f;
    public float extendDuration = 0.0f;
    //pause delegate
    public delegate void PauseStartDelegate(float pauseStrength);
    public event PauseStartDelegate OnPauseStart;

    public delegate void PauseEndDelegate();
    public event PauseEndDelegate OnPauseEnd;
    private void Awake()
    {
        //base.Awake();
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        GameManager.instance.onReset += ResetStates;
    }
    public void RequestPause(float pauseDuration, float pauseStrength)
    {
        //GetComponent<ShieldGhostAbility>().DisableAbility(pauseDuration);
        if (isPausing) return;
        StartCoroutine(PauseCoroutine(pauseDuration, pauseStrength));
    }
    private IEnumerator PauseCoroutine(float pauseDuration, float pauseStrength)
    {
        isPausing = true;
        activePauseStrength = pauseStrength;
        OnPauseStart?.Invoke(pauseStrength);
        if(PlayerControllerTest.instance.combinationIndex == 3)
        {
            PlayerControllerTest.instance.stats.SetInvincible(true);
            PlayerControllerTest.instance.stats.preventDamage = true;
        }
        yield return new WaitForSeconds(pauseDuration);
        while(extendDuration > 0.0f)
        {
            float extendTime = extendDuration;
            yield return new WaitForSeconds(extendDuration);
            extendDuration -= extendTime;
        }
        OnPauseEnd?.Invoke();
        if(PlayerControllerTest.instance.combinationIndex == 3)
        {
            PlayerControllerTest.instance.stats.SetInvincible(false);
            PlayerControllerTest.instance.stats.preventDamage = false;
        }
        // FIXME: I think this should be a invoke event thing but I don't know how to do it
        // so i will do my GPT impression :(
        isPausing = false;
    }
    public void ResetStates()
    {
        if (isPausing)
        {
            StopAllCoroutines();
            OnPauseEnd?.Invoke();
            isPausing = false;
        }
    }

}
