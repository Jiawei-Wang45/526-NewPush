using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public bool IsAttacking { get; private set; } = false;
    private void Update()
    {
        if (GameManager.instance.IsPaused || IsAttacking) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 playerToMouse= mousePosition-transform.position;
        float angle = Mathf.Atan2(playerToMouse.y, Mathf.Abs(playerToMouse.x)) * Mathf.Rad2Deg;
        Quaternion targetRotation;
        if (mousePosition.x < transform.position.x)
        {
            targetRotation = Quaternion.Euler(0, -180, angle);
        }
        else 
        {
            targetRotation = Quaternion.Euler(0, 0, angle);
        }
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        // somehow some bullets are shot without a proper initial rotation. So just stop using slerp
        transform.rotation = targetRotation;
    }
    public void SetAttacking(bool inAttacking)
    {
        IsAttacking= inAttacking;
    }

}
