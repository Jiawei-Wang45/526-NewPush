using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;

public class DefenseTutorialRoom : BaseRoom
{
    
    public enum DialogueState
    {
        DashTutorialText,
        //DashReady,
        BounceTutorialText,
        //BounceReady,
        EndDialogue,
        None
    }
    [NonSerialized] private List<BulletSpawner> bulletSpawners = new List<BulletSpawner>();
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private DialogueState state= DialogueState.None;
    [Header("DialogueText")]
    [SerializeField] private DialogueText DashTutorialText;
    [SerializeField] private DialogueText DashReady;
    [SerializeField] private DialogueText DashPrompt;
    [SerializeField] private DialogueText BounceTutorialText;
    [SerializeField] private DialogueText BounceReady;
    [SerializeField] private DialogueText BouncePrompt;
    [SerializeField] private DialogueText EndDialogue;

    [Header("Barrier")]
    [SerializeField] private GameObject rearLeftBarrier;
    [SerializeField] private GameObject rearRightBarrier;
    //[SerializeField] private GameObject middleBarrier;
    [SerializeField] private BafflePlate BafflePlate;

    [Header("NPCPos")]
    [SerializeField] private Transform NPCPos;

    [Header("ReplacedItem")]
    //[SerializeField] PlayerBaseWeapon Pistol;
    //[SerializeField] PlayerBaseWeapon Sword_NoDamage;
    [Header("SpawnedEnemy")]
    [SerializeField] GameObject spinFireEnemy;
    [SerializeField] private Transform enemyPos;
    public override void Awake()
    {
        //leftBarrier.SetActive(false);
        rightBarrier.SetActive(false);
        rearLeftBarrier.SetActive(false);
    }
    private void Start()
    {
        pc = FindFirstObjectByType<PlayerController>();
        dialogSystem.OnDialogueEnd += OnDialogueEnd;
        DialogueEventManager.instance.RegisterEvent("OnDashStart", StartDash);
        DialogueEventManager.instance.RegisterEvent("OnBounceStart", StartBounce);
    }
    public override void PlayerEntered()
    {
        TrapPlayer();
        NPC.gameObject.SetActive(true);
        pc.InteractObject = NPC;
        state = DialogueState.DashTutorialText;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(DashTutorialText);
        dialogSystem.StartDialogue("Start");
    }
    public void OnAreaEntered()
    {
        rearLeftBarrier.SetActive(true);
        EndDash();
        NPC.gameObject.transform.position = NPCPos.position;
        pc.InteractObject = NPC;
        state = DialogueState.BounceTutorialText;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(BounceTutorialText);
        dialogSystem.StartDialogue("Start");
    }

    public void StartDash()
    {
        foreach(BulletSpawner spawner in bulletSpawners)
        {
            spawner.StartEndlessShooting();
        }
        rightBarrier.SetActive(false);
        BafflePlate.SetVisibility(false);
        currentDialogueText = DashPrompt;
    }
    public void EndDash()
    {
        foreach (BulletSpawner spawner in bulletSpawners)
        {
            spawner.isShooting = false;
        }
        BafflePlate.SetVisibility(true);
    }
    public void StartBounce()
    {
        currentDialogueText = BouncePrompt;
        FindFirstObjectByType<PlayerStats>().preventDamage = true;
        Instantiate(spinFireEnemy, enemyPos.position, Quaternion.identity);

    }
    public void EndBounce()
    {
        pc.InteractObject = NPC;
        state = DialogueState.EndDialogue;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(EndDialogue);
        dialogSystem.StartDialogue("Start");
        //FindFirstObjectByType<WeaponController>().EquipNewWeapon(Pistol);  //equip pistol to let player can't kill multiple enemies during appointed time in the ability tutorial level 

    }
    public void RegisterBulletSpawner(BulletSpawner spawner)
    {
        bulletSpawners.Add(spawner);
    }
    public void OnDialogueEnd()
    {
        switch (state)
        {
            case DialogueState.DashTutorialText:
                //FindFirstObjectByType<WeaponController>().EquipNewWeapon(Pistol);
                currentDialogueText = DashReady;
                break;
            case DialogueState.BounceTutorialText:
                //FindFirstObjectByType<WeaponController>().EquipNewWeapon(Sword_NoDamage);
                currentDialogueText = BounceReady;
                break;
            case DialogueState.EndDialogue:
                rearRightBarrier.SetActive(false);
                Destroy(NPC.gameObject);
                exitRayinstance = Instantiate(exitRayEffect, exitRayPos.position, Quaternion.identity);
                break;
        }
        if (state != DialogueState.None)
        {
            pc.InteractObject = null;
            state = DialogueState.None;
        }
    }
    
}
