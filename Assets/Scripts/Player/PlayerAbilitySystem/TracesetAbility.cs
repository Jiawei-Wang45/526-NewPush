using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// TracesetAbility: a pause-type ability where during the pause the player is invincible and cannot fire
public class TracesetAbility : MonoBehaviour
{
    private PlayerControllerTest pc;
    private PlayerStats stats;
    // pause parameters
    [Header("Traceset Pause Parameters")]
    public float activePauseDuration = 5.0f;
    public float activePauseCooldown = 3.0f;
    public float activePauseStrength = 20.0f;

    private bool isCooldown = false;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        // try to find player controller on same gameObject first
        pc = GetComponent<PlayerControllerTest>();
        if (pc == null)
            pc = FindFirstObjectByType<PlayerControllerTest>();
        if (pc != null)
            stats = pc.GetComponent<PlayerStats>();

        // bind input pause action and reset callback
        if (pc != null)
        {
            try
            {
                pc.playerInput.Default.Pause.performed += ctx => ActivatePause(activePauseDuration, activePauseStrength, activePauseCooldown);
            }
            catch { }
            try
            {
                GameManager.instance.onReset += ResetLocal;
            }
            catch { }
        }
    }

    private void HandlePauseStart(float pauseStrength)
    {
        // ensure we have references
        if (pc == null)
            pc = FindFirstObjectByType<PlayerControllerTest>();
        if (pc == null) return;

        stats = pc.GetComponent<PlayerStats>();
        // set invincible via layer swap (same approach as InvincibleAbility)
        try
        {
            gameObject.layer = LayerMask.NameToLayer("Invincible");
        }
        catch { }

        // disable firing input actions to prevent player firing during pause
        try
        {
            var input = pc.playerInput;
            if (input != null)
            {
                input.Default.Fire.Disable();
                // also disable any special fire actions if present
                if (input.Default.SpecialBullet != null) input.Default.SpecialBullet.Disable();
            }
        }
        catch { }
    }

    private void HandlePauseEnd()
    {
        if (pc == null)
            pc = FindFirstObjectByType<PlayerControllerTest>();
        if (pc == null) return;

        stats = pc.GetComponent<PlayerStats>();
        // restore layer back to Player
        try
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        }
        catch { }

        try
        {
            var input = pc.playerInput;
            if (input != null)
            {
                input.Default.Fire.Enable();
                if (input.Default.SpecialBullet != null) input.Default.SpecialBullet.Enable();
            }
        }
        catch { }
    }

    private void ResetLocal()
    {
        // stop any running coroutine and restore state
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }
        HandlePauseEnd();
        try { gameObject.layer = LayerMask.NameToLayer("Player"); } catch { }
        isCooldown = false;
    }

    // Public API to activate traceset pause (similar contract to PauseAbility)
    public void ActivatePause(float pauseDuration, float pauseStrength, float pauseCooldown)
    {
        if (isCooldown) return;
        isCooldown = true;
        // start the traceset coroutine and keep reference so we can cancel on reset
        activeCoroutine = StartCoroutine(TracesetCoroutine(pauseDuration, pauseStrength, pauseCooldown));
    }

    private IEnumerator TracesetCoroutine(float pauseDuration, float pauseStrength, float pauseCooldown)
    {
        isCooldown = true;

        // Trigger local pause start behavior (invincibility, disable fire)
        HandlePauseStart(pauseStrength);

        // Also delegate to global PauseAbility (so enemies/bullets subscribed to it will pause)
        try
        {
            // trigger global pause events without analytics or PauseAbility cooldown
            if (PauseManager.instance != null)
            {
                PauseManager.instance.RequestPause(pauseDuration, pauseStrength);
            }
        }
        catch { }

        float timeElapsed = 0f;
        while (timeElapsed < pauseDuration)
        {
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // End local pause behavior
        HandlePauseEnd();

        // wait cooldown
        yield return new WaitForSeconds(pauseCooldown);
        try { AudioManager.instance.PlaySound("cooldownFinish"); } catch { }
        isCooldown = false;
        activeCoroutine = null;
        // ensure local cleanup
        ResetLocal();
    }
}
