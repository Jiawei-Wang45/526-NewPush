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
    private PlayerStats stats;
    public PlayerInput playerInput;
    public int combinationIndex = 0;
    // movement parameter
    public float speed;
    public Vector2 movement;
    public Vector2 knockback;
    // revive parameter
    public Vector2 initialPosition;
    //Analytics
    // [SerializeField] public SendToGoogle sendToGoogle;
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

        //GameManager.instance.onReset += Reset;
    }

    public void AddForcePlayer(Vector2 force)
    {
        knockback += force;
        stats.SetInvincible(true);
        stats.preventDamage = true;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed + knockback;
        knockback -= knockback * 0.05f;
        if((combinationIndex == 6 || combinationIndex == 7) && knockback.magnitude < 1.0f)
        {
            stats.SetInvincible(false);
            stats.preventDamage = false;
        }
    }

    public void TakeDamage(float damage, HSLColor bulletColor)
    {
        stats.TakeDamage(damage, bulletColor);
    }
    private void OnMoveTriggered(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>();
    }
    //public void Reset()
    //{
    //    transform.position = initialPosition;
    //}

    public void UponWaveClear()
    {
        stats.ChangeHealth(stats.maxHealth);
        initialPosition = transform.position;
    }
    //private void OnDestroy()
    //{
    //    if (GameManager.instance != null)
    //    {
    //        GameManager.instance.onReset -= Reset;
    //    }
    //}

}
