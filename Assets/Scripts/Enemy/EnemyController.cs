using UnityEngine;

public class EnemyController : MonoBehaviour
{

    public PlayerControllerTest pcTest;
    public EnemyStats enemyStats;
    public Rigidbody2D rb;
    public GameManager gameManager;
    public GameObject BoundHealthbar;
    public float RotationSpeed = 15.0f;
    public float enemySpeed;
    private Vector2 movement;

    public bool isAttacking;


    private void Start()
    {
        pcTest = FindFirstObjectByType<PlayerControllerTest>();
        enemyStats=GetComponent<EnemyStats>();
        rb = GetComponent<Rigidbody2D>();
        gameManager = FindFirstObjectByType<GameManager>();
        RefreshStats();
    }
    public void RefreshStats()
    {
        enemySpeed = enemyStats.enemyMovementSpeed;
    }
    private void Update()
    {
        if (gameManager.isPlayerAlive)
        {
            Vector2 direction = pcTest.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

            if (isAttacking)
            {
                pcTest.TakeDamage(enemyStats.enemyDamage);
            }
        }
    }
    private void FixedUpdate()
    {
        if (gameManager.isPlayerAlive)
        {
            rb.linearVelocity = transform.right * enemySpeed;
        }
            
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerControllerTest>(out PlayerControllerTest pcScript))
        {
            isAttacking = false;
        }
    }
}
