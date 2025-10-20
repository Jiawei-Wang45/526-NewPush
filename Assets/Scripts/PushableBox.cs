using UnityEngine;

public class PushableBox : MonoBehaviour
{
    public float pushForce = 2f;
    public float maxSpeed = 1f;
    public float dragForce = 5f;
    
    private Rigidbody2D rb;
    private bool isBeingPushed = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerControllerTest player = collision.gameObject.GetComponent<PlayerControllerTest>();
            
            if (player != null)
            {
                float inputX = Input.GetAxisRaw("Horizontal");
                float inputY = Input.GetAxisRaw("Vertical");
                Vector2 playerInput = new Vector2(inputX, inputY);
                
                if (playerInput.magnitude > 0.1f)
                {
                    Vector2 pushDirection = (transform.position - collision.transform.position).normalized;
                    
                    float dotProduct = Vector2.Dot(playerInput.normalized, pushDirection);
                    if (dotProduct > 0.5f)
                    {
                        PushBox(pushDirection);
                        isBeingPushed = true;
                    }
                    else
                    {
                        isBeingPushed = false;
                    }
                }
                else
                {
                    isBeingPushed = false;
                }
            }
        }
    }
    
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isBeingPushed = false;
        }
    }
    
    void PushBox(Vector2 direction)
    {
        if (rb.linearVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * pushForce, ForceMode2D.Force);
        }
        
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    void FixedUpdate()
    {
        if (!isBeingPushed)
        {
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                rb.AddForce(-rb.linearVelocity * dragForce, ForceMode2D.Force);
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}