
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IDamagable
{
    //components
    public static PlayerController instance;
    [NonSerialized] private Rigidbody2D rb;
    [NonSerialized] private PlayerStats stats;
    [NonSerialized] private WeaponController weaponController;
    public PlayerInput playerInput;
    public int combinationIndex = 0;
    // movement parameter
    public float speed=10.0f;
    public Vector2 movement;
    public Vector2 knockback;
    // revive parameter
    public Vector2 initialPosition;

    //Pickup items
    public IInteractable InteractObject = null;
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
        weaponController = GetComponent<WeaponController>();
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
        //player movement binding, ability binding now moves to relative ability script
        playerInput.Default.Move.performed += OnMoveTriggered;
        playerInput.Default.Move.canceled += OnMoveTriggered;
        playerInput.Default.Interact.started += OnInteractTriggered;
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

    public void TakeDamage(float damage)
    {
        stats.TakeDamage(damage);
    }
    private void OnMoveTriggered(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>().normalized;
    }

    public void UponWaveClear()
    {
        stats.ChangeHealth(stats.maxHealth);
        initialPosition = transform.position;
    }
    public void OnWeaponChanged(PlayerBaseWeapon newWeapon)
    {
        weaponController.ChangeWeapon(newWeapon);
    }
    public void SetInteractObject(IInteractable InObject)
    {
        InteractObject = InObject;
    }
    public IInteractable GetInteractObject()
    {
        return InteractObject;
    }
    private void OnInteractTriggered(InputAction.CallbackContext context)
    {
        if (InteractObject!=null)
        {
            InteractObject.Interact();
        }
    }
}
