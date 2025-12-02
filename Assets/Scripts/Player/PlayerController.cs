
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
    [NonSerialized] private Animator anim; 
    [NonSerialized] private SpriteRenderer sr; 
    public PlayerInput playerInput;
    public int weaponIndex = -1;
    public int abilityIndex = -1;
    public int combinationIndex = -1; 
    // movement parameter
    public float speed=10;
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
        anim = GetComponent<Animator>(); 
        sr = GetComponent<SpriteRenderer>();
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
        // Use the cached SpriteRenderer reference (sr) to change alpha instead of searching each time
        Color color = sr != null ? sr.color : GetComponentInParent<SpriteRenderer>().color;
        color.a = 0.5f;
        if (sr != null) sr.color = color;
        else GetComponentInParent<SpriteRenderer>().color = color;
        stats.preventDamage = true;
    }

    private void Update()
    {
        // Control animation: set isWalking to true if player is moving
        if (movement.sqrMagnitude > 0)
        {
            anim.SetBool("isWalking", true);
        }
        else
        {
            anim.SetBool("isWalking", false);
        }

        // Control sprite flipping based on horizontal movement
        // Use SpriteRenderer.flipX so only the sprite is mirrored, not the entire transform
        if (movement.x > 0)
        {
            // Moving right: do not flip the sprite (facing right)
            if (sr != null) sr.flipX = false;
        }
        else if (movement.x < 0)
        {
            // Moving left: flip the sprite horizontally (facing left)
            if (sr != null) sr.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * speed + knockback;
        knockback -= knockback * 0.05f;
        if((combinationIndex == 6 || combinationIndex == 7) && knockback.magnitude < 3.0f)
        {
            stats.SetInvincible(false);
            // Use the cached SpriteRenderer reference (sr) to change alpha
            Color color = sr != null ? sr.color : GetComponentInParent<SpriteRenderer>().color;
            color.a = 1.0f;
            if (sr != null) sr.color = color;
            else GetComponentInParent<SpriteRenderer>().color = color;
            stats.preventDamage = false;
            knockback *= 0.0f;
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
