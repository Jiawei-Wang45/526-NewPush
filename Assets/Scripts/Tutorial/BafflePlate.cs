using UnityEngine;
using UnityEngine.Tilemaps;

public class BafflePlate : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;
    private string bulletTag;
    private void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
        //bulletTag = "TutorialBullet";
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{
    //    if (collision.gameObject.CompareTag(bulletTag))
    //    {
    //        Destroy(collision.gameObject);
    //    }
    //}
    public void SetVisibility(bool isVisible)
    {
        tilemapRenderer.enabled = isVisible;
    }
}
