using System.Collections;

using UnityEngine;

public class Heart : BaseItem
{
    public float healAmount = 1.0f;
    public Transform player;
    public float thresholdDistance=5.0f;
    public float movingSpeed = 6.0f;
    public float rotationSpeed = 5.0f;
    private bool isFollowing = false;
    private void Start()
    {
        player = FindFirstObjectByType<PlayerControllerTest>().transform;
    }
    private void Update()
    {
        if (!isFollowing)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance < thresholdDistance)
            {
                isFollowing = true;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, movingSpeed * Time.deltaTime);
            Vector2 direction = player.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90.0f;
            Quaternion targetrotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetrotation, rotationSpeed * Time.deltaTime);
        }
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerControllerTest player = collision.gameObject.GetComponent<PlayerControllerTest>();
        if (player != null)
        {
            player.TakeDamage(-healAmount, new HSLColor());
            AudioManager.instance.PlaySound("heal");
            Destroy(gameObject);
        }
    }
  
}
