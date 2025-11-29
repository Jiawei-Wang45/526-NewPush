using System.Collections;

using UnityEngine;

public class Heart : ConsumableItem
{
    public float healAmount = 2.0f;
    public float thresholdDistance=5.0f;
    public float movingSpeed = 6.0f;
    public float rotationSpeed = 5.0f;
    private bool isFollowing = false;
    protected override void Start()
    {
        base.Start();
    }
    private void Update()
    {
        if (!isFollowing)
        {
            float distance = Vector2.Distance(transform.position, pc.transform.position);
            if (distance < thresholdDistance)
            {
                isFollowing = true;
            }
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, pc.transform.position, movingSpeed * Time.deltaTime);
            Vector2 direction = pc.transform.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90.0f;
            Quaternion targetrotation = Quaternion.AngleAxis(angle, Vector3.forward);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetrotation, rotationSpeed * Time.deltaTime);
        }
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.gameObject.layer==LayerMask.NameToLayer("Player"))
        {
            pc.TakeDamage(-healAmount);
            AudioManager.instance.PlaySound("heal");
            Destroy(gameObject);
        }
    }
  
}
