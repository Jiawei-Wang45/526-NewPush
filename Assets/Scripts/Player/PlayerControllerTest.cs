using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;
public class PlayerControllerTest : MonoBehaviour, IDamagable
{
    //components
    public static PlayerControllerTest instance;
    private Rigidbody2D rb;
    public PlayerStats stats;
    public PlayerInput playerInput;
    // movement parameter
    public float speed;
    public Vector2 movement;
    // revive parameter
    public Vector2 initialPosition;
    //Analytics
    [Header("Analytics")]
    [SerializeField] public SendToGoogle sendToGoogle;
    // delegate 
    public delegate void RestCalledDelegate();
    public event RestCalledDelegate OnResetCalled;
    private void Awake()
    {
        if (instance==null)
        {
            instance=this;
        }
        else
        {
            Destroy(gameObject);
        }
        playerInput = new PlayerInput();
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        // ensure sendToGoogle is assigned
        if (sendToGoogle == null)
        {
            // try to find any enabled instance first
            sendToGoogle = FindFirstObjectByType<SendToGoogle>();
            // if still null, try to find inactive instances (Unity API that returns array)
            if (sendToGoogle == null)
            {
                try
                {
                    SendToGoogle[] all = FindObjectsByType<SendToGoogle>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                    if (all != null && all.Length > 0)
                    {
                        sendToGoogle = all[0];
                    }
                }
                catch
                {
                    // fallback: use FindObjectOfType that supports inactive when available
                    try
                    {
                        sendToGoogle = FindObjectOfType<SendToGoogle>(true);
                    }
                    catch { }
                }
            }
        }

        Debug.Log($"[PlayerControllerTest] Awake auto-assign sendToGoogle: {sendToGoogle != null} (object: {sendToGoogle?.gameObject.name})");
    }
    private void OnEnable()
    {
        playerInput.Default.Enable();
    }
    private void OnDisable()
    {
        playerInput.Default.Disable();
    }
    private void Start()
    { 
        initialPosition = transform.position;
        speed = stats.movementSpeed;
        //player movement binding, ability binding now moves to relative ability script
        playerInput.Default.Move.performed += OnMoveTriggered;
        playerInput.Default.Move.canceled += OnMoveTriggered;
    }


    private void Update()
    {
        UpdatePlayerColor();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed;
    }

    public void TakeDamage(float damage, HSLColor bulletColor)
    {
        stats.TakeDamage(damage, bulletColor);
    }
    private void OnMoveTriggered(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }

    private void UpdatePlayerColor()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color baseColor = stats.playerColor.ToRGB();
            if (stats != null && stats.isInvincible)
            {
                // half transparent when invincible
                baseColor.a = 0.5f;
            }
            else
            {
                baseColor.a = 1f;
            }
            spriteRenderer.color = baseColor;
        }
    }
    public void Reset()
    {
        // event broadcast, each ability component will receive 
        OnResetCalled?.Invoke();
        transform.position = initialPosition;
    }

    public void UponWaveClear()
    {
        stats.ChangeHealth(stats.maxHealth);
        initialPosition = transform.position;
    }

    //private void OnAbilityTriggered(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //    {
    //        switch (abilityEnum)
    //        {
    //            case 0:
    //                PauseAllPausable(2.0f, 8.0f);
    //                break;
    //            case 1:
    //                PauseAllPausable(2.0f, 0.1f);
    //                break;

    //            case 3:
    //                ActiveRecordGhost();
    //                break;
    //            case 4:
    //                GhostDash();
    //                break;
    //            default:
    //                break;
    //        }

    //        // Send analytics when ability is used
    //        GameManager gm = FindFirstObjectByType<GameManager>();
    //        int waveToSend = gm != null ? gm.CurrentWave : 0;
    //        Debug.Log($"[PlayerControllerTest] sendToGoogle assigned? {sendToGoogle != null} wave={waveToSend} pos={transform.position}");
    //        if (sendToGoogle != null)
    //        {
    //            sendToGoogle.SendAbilityUse(transform.position, waveToSend);
    //        }
    //    }
    //}

}
