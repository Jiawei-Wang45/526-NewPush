using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

public class GhostController : MonoBehaviour
{

    private int stateIndex = 0;
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    public Vector2 initialPosition;
    private Color initialColor;
    private List<ObjectState> recordedStates = new List<ObjectState>();

    public Transform ghostAim;
    public Transform firePoint;
    public PlayerWeapon currentWeapon;
    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        initialPosition = transform.position;
        initialColor = sr.color;

    }
    
    public void InitializeGhost(Vector2 position, List<ObjectState> playerStates)
    {
        Debug.Log("Initializing");
        transform.position = position;
        recordedStates = playerStates;

    }

    private void FixedUpdate()
    {
        if (stateIndex < recordedStates.Count)
        {
            ObjectState currentState = recordedStates[stateIndex];
            rb.linearVelocity = currentState.currentVelocity;
            ghostAim.rotation = currentState.currentRotation;
            if (currentState.usingNewWeapon)
            {
                currentWeapon = currentState.usingNewWeapon;
                Debug.Log("Equipping weapon");
            }
            if (currentState.currentlyFiring && currentWeapon)
            {
                Fire();
            }
            stateIndex++;
        } else
        {
            Debug.Log("Disappearing");
            StartCoroutine(Disappear(0.4f));
        }
    }

    private IEnumerator Disappear(float fadeDuration)
    {
        GetComponent<Collider2D>().enabled = false;
        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0.5f, 0f, timeElapsed / fadeDuration);
            sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, newAlpha);
            yield return null;
        }

        sr.color = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        gameObject.SetActive(false);
    }

    private void Fire()
    {
        float bulletTiltAngle = -(currentWeapon.weaponBulletAmount - 1) * currentWeapon.weaponFiringAngle / 2;
        for (int i = 0;i<currentWeapon.weaponBulletAmount;i++)
        {
            GameObject spawnedBullet=Instantiate(currentWeapon.bulletType, firePoint.position, firePoint.rotation);
            Bullet_Default bulletAttributes = spawnedBullet.GetComponent<Bullet_Default>();
            bulletAttributes.InitBullet(currentWeapon.weaponBulletSpeed, currentWeapon.weaponBulletLifeTime, currentWeapon.weaponBulletDamage, "player");
            spawnedBullet.transform.Rotate(0, 0, bulletTiltAngle+Random.Range(-currentWeapon.weaponBulletSpread,currentWeapon.weaponBulletSpread));
            // Make the bullet semi-transparent like the ghost
            SpriteRenderer bulletSr = spawnedBullet.GetComponent<SpriteRenderer>();
            if (bulletSr != null)
            {
                Color bulletColor = bulletSr.color;
                bulletSr.color = new Color(bulletColor.r, bulletColor.g, bulletColor.b, 0.5f);
            }
            bulletTiltAngle += currentWeapon.weaponFiringAngle;
        }
        
    }
    public void Reset()
    {
        rb.linearVelocityX = 0;
        rb.linearVelocityY = 0;
        StopAllCoroutines();
        gameObject.SetActive(true);
        stateIndex = 0;
        transform.position = initialPosition;
        sr.color = initialColor;
    }

}
