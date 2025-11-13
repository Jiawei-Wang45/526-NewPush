using UnityEngine;

public class Bullet_Tutorial : MonoBehaviour
{
    public float bulletSpeed;
    private float cachedSpeed;
    private void Awake()
    {
        cachedSpeed = bulletSpeed;
    }
    private void Start()
    {
        PauseManager.instance.OnPauseStart += PauseStart;
        PauseManager.instance.OnPauseEnd += PauseEnd;
    }
    private void Update()
    {
        transform.Translate(Vector3.left * bulletSpeed * Time.deltaTime);
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
    //    {
    //        Destroy(gameObject);
    //    }
    //}
    public void PauseStart(float pauseStrength)
    {
        bulletSpeed /= pauseStrength;
    }
    public void PauseEnd()
    {
        bulletSpeed = cachedSpeed;
    }
    protected virtual void OnDestroy()
    {
        PauseManager.instance.OnPauseStart -= PauseStart;
        PauseManager.instance.OnPauseEnd -= PauseEnd;
    }
}
