using System;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private Transform inventory;
    [SerializeField] private WeaponItem droppedweaponPrefab;
    [NonSerialized] private Transform weaponHolder;
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private MeleeAbility meleeAbility;
    [NonSerialized] private FireAbility fireAbility;
    [NonSerialized] private GameManager gameManager;
    public IWeapon IcurrentAbility;
    
    private const int maxWeaponCount = 6;  //temporarily set to 6, matching nums of the box
    private PlayerBaseWeapon[] weaponList;
    private GameObject weaponInstance;
    private int cachedSlotIndex;

    private void Awake()
    {
        pc = GetComponent<PlayerController>();
        meleeAbility=GetComponent<MeleeAbility>();
        fireAbility=GetComponent<FireAbility>();
        cachedSlotIndex = maxWeaponCount - 1;
        weaponHolder = transform.Find("PlayerAim/WeaponHolder");
        weaponList =new PlayerBaseWeapon[maxWeaponCount];   
    }
    private void Start()
    {
        gameManager = GameManager.instance;
        pc.playerInput.Default.LeftMouse.started += OnLeftMouseTriggered;
        pc.playerInput.Default.LeftMouse.canceled += OnLeftMouseReleased;
        pc.playerInput.Default.RightMouse.started += OnRightMouseTriggered;
        pc.playerInput.Default.Reload.started += OnReloadTriggered;
        pc.playerInput.Default.ChangeWeapon.started += OnWeaponChanged;
        pc.playerInput.Default.DropWeapon.performed += OnWeaponDropped;
    }
    #region callback
    private void OnLeftMouseTriggered(InputAction.CallbackContext context)
    {
        if (!gameManager.IsPaused)
            IcurrentAbility?.LeftMouseTriggered();
    }
    private void OnLeftMouseReleased(InputAction.CallbackContext context)
    {
        if (!gameManager.IsPaused)
            IcurrentAbility?.LeftMouseReleased();
    }
    private void OnRightMouseTriggered(InputAction.CallbackContext context)
    {
        if (!gameManager.IsPaused)
            IcurrentAbility?.RightMouseTriggered();
    }
    private void OnReloadTriggered(InputAction.CallbackContext context)
    {
        if (!gameManager.IsPaused)
            IcurrentAbility?.ReloadTriggered();
    }
    #endregion callback
    private void OnWeaponChanged(InputAction.CallbackContext context)
    {
        ChangeWeapon((int)context.ReadValue<float>(),false);
    }
    private void OnWeaponDropped(InputAction.CallbackContext context)
    {
        DropWeapon();
    }
    public bool EquipNewWeapon(PlayerBaseWeapon newWeapon)
    {
        int emptyIndex = FindFirstEmptySlot();
        if (emptyIndex == -1) return false;
        weaponList[emptyIndex] = newWeapon;

        ChangeWeapon(emptyIndex,true);
        return true;
    }
    public void ChangeWeapon(int index,bool updateIcon)
    {
        if (cachedSlotIndex == index || weaponList[index] == null) return;
        PlayerBaseWeapon weaponToEquip= weaponList[index];
        if (weaponInstance != null)
        {
            Destroy(weaponInstance);
        }
        weaponInstance = Instantiate(weaponToEquip.weaponPrefab, weaponHolder.position, weaponHolder.rotation, weaponHolder);
        if (weaponToEquip.weaponClass != WeaponClass.Melee)
        {
            IcurrentAbility = fireAbility;
            fireAbility.ChangeWeapon((RangedWeapon)weaponToEquip, weaponInstance.transform.GetChild(0), weaponInstance.GetComponent<Animator>());
        }
        else
        {
            IcurrentAbility = meleeAbility;
            meleeAbility.ChangeWeapon((MeleeWeapon)weaponToEquip, weaponInstance.transform.GetChild(0), weaponInstance.GetComponent<Animator>());
        }
        //UI update
        if (updateIcon)
        {
            GameObject weaponIcon = inventory.GetChild(index).GetChild(1).gameObject;
            weaponIcon.SetActive(true);
            weaponIcon.GetComponent<Image>().sprite = weaponInstance.GetComponent<SpriteRenderer>().sprite;

        }
        HighlightSlot(index);
    }
    private int FindFirstEmptySlot()
    {
        for (int i=0;i< weaponList.Length;i++)
        {
            if (weaponList[i] == null)
                return i;
        }
        return -1;
    }
    private int FindFirstNonEmptySlot()
    {
        for (int i = 0; i < weaponList.Length; i++)
        {
            if (weaponList[i] != null)
                return i;
        }
        return -1;
    }
    private void HighlightSlot(int index)
    {
        // change the value of cachedSlotIndex in this function to correctly unhighlight the previous slot.
        inventory.GetChild(cachedSlotIndex).GetChild(0).gameObject.SetActive(false);
        cachedSlotIndex = index;
        inventory.GetChild(cachedSlotIndex).GetChild(0).gameObject.SetActive(true);

    }
    private void DropWeapon()
    {
        if (weaponList[cachedSlotIndex] == null) return;  // this only happens when we have no weapon, otherwise cachedSlotIndex will always point to a valid weapon
        PlayerBaseWeapon weaponToDrop= weaponList[cachedSlotIndex];
        weaponList[cachedSlotIndex] = null;

        GameObject weaponIcon = inventory.GetChild(cachedSlotIndex).GetChild(1).gameObject;
        weaponIcon.SetActive(false);
        weaponIcon.GetComponent<Image>().sprite = null;

        float angle = UnityEngine.Random.Range(0, 2 * Mathf.PI);
        Vector3 dropPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        WeaponItem droppedWeapon = Instantiate(droppedweaponPrefab, dropPos, Quaternion.identity);
        droppedWeapon.InitWeapon(weaponToDrop, weaponInstance.GetComponent<SpriteRenderer>().sprite);

        int nonEmptyIndex = FindFirstNonEmptySlot();
        if (nonEmptyIndex != -1)
        {   
            ChangeWeapon(nonEmptyIndex,false);
        }
        else
        {
            IcurrentAbility = null;
            inventory.GetChild(cachedSlotIndex).GetChild(0).gameObject.SetActive(false);
            Destroy(weaponInstance);
            weaponInstance = null;
            cachedSlotIndex = maxWeaponCount - 1;
        }

        
    }
}
