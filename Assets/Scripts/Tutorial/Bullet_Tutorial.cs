using UnityEngine;

public class Bullet_Tutorial : MonoBehaviour
{
    private static readonly string baffleTag="Baffle";
    public float bulletSpeed;
    //private float cachedSpeed;
    private Rigidbody2D rb;
    private void Awake()
    {
        rb=GetComponent<Rigidbody2D>();
        //cachedSpeed = bulletSpeed;
    }
    //private void Start()
    //{
    //    PauseManager.instance.OnPauseStart += PauseStart;
    //    PauseManager.instance.OnPauseEnd += PauseEnd;
    //}
    private void Start()
    {
        rb.linearVelocity = Vector2.left * bulletSpeed;
    }
    //private void FixedUpdate()
    //{
    //    rb.MovePosition(rb.position + Vector2.left * bulletSpeed * Time.fixedDeltaTime);
    //}
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.gameObject.CompareTag(baffleTag))
            Destroy(gameObject);
    }
    //private void Update()
    //{

    //    transform.Translate(Vector3.left * bulletSpeed * Time.deltaTime);
    //}
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    //public void PauseStart(float pauseStrength)
    //{
    //    bulletSpeed /= pauseStrength;
    //}
    //public void PauseEnd()
    //{
    //    bulletSpeed = cachedSpeed;
    //}
    //protected virtual void OnDestroy()
    //{
    //    PauseManager.instance.OnPauseStart -= PauseStart;
    //    PauseManager.instance.OnPauseEnd -= PauseEnd;
    //}
}
