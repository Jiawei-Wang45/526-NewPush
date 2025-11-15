using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    // two point might be misleading, since the scale of these two points are not the same cause we don't have the correct assets, the only way to adjust the shape of the square is to change the sale. In later, 
    // if we have the texture which has proper size, we can set the scale all to 1 and just keep one "attack point" instead
    [SerializeField] private GameObject attackPoint;
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private MeleeAbility meleeAbility;
    [NonSerialized] private FireAbility fireAbility;
    public IWeapon IcurrentWeapon;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        meleeAbility=GetComponent<MeleeAbility>();
        fireAbility=GetComponent<FireAbility>();
    }
    private void Start()
    {
        pc.playerInput.Default.LeftMouse.started += OnLeftMouseTriggered;
        pc.playerInput.Default.LeftMouse.canceled += OnLeftMouseReleased;
        pc.playerInput.Default.RightMouse.started += OnRightMouseTriggered;
        pc.playerInput.Default.Reload.started += OnReloadTriggered;
    }
    #region callback
    private void OnLeftMouseTriggered(InputAction.CallbackContext context)
    {
        IcurrentWeapon?.LeftMouseTriggered();
    }
    private void OnLeftMouseReleased(InputAction.CallbackContext context)
    {
        IcurrentWeapon?.LeftMouseReleased();
    }
    private void OnRightMouseTriggered(InputAction.CallbackContext context)
    {
        IcurrentWeapon?.RightMouseTriggered();
    }
    private void OnReloadTriggered(InputAction.CallbackContext context)
    {
        IcurrentWeapon?.ReloadTriggered();
    }
    #endregion callback

    public void ChangeWeapon(PlayerBaseWeapon newWeapon)
    {
        if (newWeapon.weaponClass!=WeaponClass.Melee)
        {
            attackPoint.GetComponent<SpriteRenderer>().sprite = newWeapon.weaponTexture;
            IcurrentWeapon = fireAbility;
            fireAbility.ChangeWeapon((BaseWeapon)newWeapon);
        }
        else
        {

            attackPoint.GetComponent<SpriteRenderer>().sprite = newWeapon.weaponTexture;
            IcurrentWeapon = meleeAbility;
            meleeAbility.ChangeWeapon((MeleeWeapon)newWeapon);
        }
    }
}
