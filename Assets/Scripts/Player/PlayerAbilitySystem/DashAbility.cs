using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class DashAbility : MonoBehaviour
{
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private TrailRenderer trailRenderer;
    [NonSerialized] private SpriteRenderer spriteRenderer;
    [NonSerialized] private float cachedSpeed;
    [NonSerialized] private Material cachedMa;
    [SerializeField] Material dashMa;
    [SerializeField] private float dashMultiplier;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCD;
    private bool isDashing=false;
    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        cachedSpeed = pc.speed;
        cachedMa = gameObject.GetComponent<SpriteRenderer>().material;
        trailRenderer =GetComponent<TrailRenderer>();
        spriteRenderer=GetComponent<SpriteRenderer>();
    }
    public void ActivateDash()
    {
        if (!isDashing && pc.movement!=Vector2.zero)
        {
            isDashing = true;
            StartCoroutine(DashCoroutine(pc.movement));
        }
    }
    private IEnumerator DashCoroutine(Vector2 cachedMovement)
    {
        pc.playerInput.Default.Move.Disable();
        pc.movement= cachedMovement;
        pc.speed = cachedSpeed * dashMultiplier;
        trailRenderer.emitting = true;
        gameObject.layer= LayerMask.NameToLayer("Invincible");
        spriteRenderer.material = dashMa;
        yield return new WaitForSeconds(dashTime);
        pc.playerInput.Default.Move.Enable();
        pc.speed = cachedSpeed;
        trailRenderer.emitting = false;
        gameObject.layer= LayerMask.NameToLayer("Player");
        spriteRenderer.material = cachedMa;
        pc.movement = Vector2.zero;
        yield return new WaitForSeconds(dashCD);
        isDashing = false;
    }

}
