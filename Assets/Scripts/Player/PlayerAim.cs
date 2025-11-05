using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    public float RotationSpeed = 15.0f;
    private void Update()
    {
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
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    }

}
