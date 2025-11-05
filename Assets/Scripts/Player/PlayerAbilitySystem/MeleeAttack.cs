using UnityEngine;
using UnityEngine.InputSystem;

public class MeleeAttack: MonoBehaviour
{
    private PlayerControllerTest pc;
    private Animator animator;
    [SerializeField] private GameObject SlashEffectPrefab;

    private GameObject SlashEffectInstance;
    private void Awake()
    {
        pc = GetComponentInParent<PlayerControllerTest>();
        animator= GetComponent<Animator>();
    }
    private void Start()
    {
        pc.playerInput.Default.Attack.started += Attack;
    }
    private void Attack(InputAction.CallbackContext context)
    {
        animator.SetTrigger("Attack");
        Instantiate(SlashEffectPrefab, transform.position, transform.rotation,transform.parent);
    }
}
