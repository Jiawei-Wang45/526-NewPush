using System.Collections;
using UnityEngine;

public class PauseManager : MonoBehaviour
{
    //components
    public static PauseManager instance;

    //pause variables
    private bool isPausing = false;
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
        OnPauseStart?.Invoke(pauseStrength);
        yield return new WaitForSeconds(pauseDuration);
        OnPauseEnd?.Invoke();
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
