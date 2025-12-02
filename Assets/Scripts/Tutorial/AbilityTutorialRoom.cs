using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilityTutorialRoom : BaseRoom
{
    public enum DialogueState
    {
        ClusterBombTutorial,
        OnClusterBombPickup,
        InvincibleDashTutorial,
        OnInvincibleDashPickup,
        EndDialogue,
        None
    }
    [NonSerialized] private List<BulletSpawner> bulletSpawners = new List<BulletSpawner>();
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private DialogueState state= DialogueState.None;
    [Header("DialogueText")]
    [SerializeField] private DialogueText ClusterBombTutorial;
    [SerializeField] private DialogueText PickupPrompt;
    [SerializeField] private DialogueText OnClusterBombPickup;
    [SerializeField] private DialogueText ClusterBombTriggerPrompt;
    [SerializeField] private DialogueText InvincibleDashTutorial;
    [SerializeField] private DialogueText OnInvincibleDashPickup;
    [SerializeField] private DialogueText ReadyToDash;
    [SerializeField] private DialogueText InvincibleDashPrompt;
    [SerializeField] private DialogueText EndDialogue;

    [Header("Barrier")]
    [SerializeField] private BafflePlate BafflePlate;
    [SerializeField] private GameObject rearLeftBarrier;
    [SerializeField] private GameObject rearRightBarrier;
    [Header("NPCPos")]
    [SerializeField] private Transform NPCPos;

    [Header("Ability")]
    [SerializeField] private Transform AbilityPos;
    [SerializeField] private GameObject ClusterBomb;
    [SerializeField] private GameObject InvincibleDash;
    [Header("EnemyManager")]
    [SerializeField] private EnemyManager_Tutorial enemyManager;

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
        DialogueEventManager.instance.RegisterEvent("OnInvincibleDashStart", StartInvincibleDash);
    }

    public override void PlayerEntered()
    {
        TrapPlayer();
        NPC.gameObject.SetActive(true);
        pc.InteractObject = NPC;
        state = DialogueState.ClusterBombTutorial;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(ClusterBombTutorial);
        dialogSystem.StartDialogue("Start");
    }
    public void OnAreaEntered()
    {
        //switch(areaType)
        //{
        //    case TutorialArea.Area.frontPart:
        //        TrapPlayer();
        //        NPC.gameObject.SetActive(true);
        //        pc.InteractObject = NPC;
        //        state = DialogueState.ClusterBombTutorial;
        //        dialogSystem.gameObject.SetActive(true);
        //        dialogSystem.SetDialogueText(ClusterBombTutorial);
        //        dialogSystem.StartDialogue("Start");
        //        break;
        //    case TutorialArea.Area.rearPart:
                
        //}
        //EndInvincibleDash();
        rearLeftBarrier.SetActive(true);
        NPC.gameObject.transform.position = NPCPos.position;
        pc.InteractObject = NPC;
        state = DialogueState.EndDialogue;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(EndDialogue);
        dialogSystem.StartDialogue("Start");
    }
    public void OnEnemyDestroyed(int index)
    {
        enemyManager.OnEnemyDestroyed(index);

    }
    public void OnAllEnemiesCleared()
    {
        pc.InteractObject = NPC;
        state = DialogueState.InvincibleDashTutorial;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(InvincibleDashTutorial);
        dialogSystem.StartDialogue("Start");
    }
    public void OnClusterBombPicked()
    {
        pc.InteractObject = NPC;
        state = DialogueState.OnClusterBombPickup;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(OnClusterBombPickup);
        dialogSystem.StartDialogue("Start");
    }
    public void OnInvincibleDashPicked()
    {
        pc.InteractObject = NPC;
        state = DialogueState.OnInvincibleDashPickup;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(OnInvincibleDashPickup);
        dialogSystem.StartDialogue("Start");
    }

    public void StartInvincibleDash()
    {
        foreach (BulletSpawner spawner in bulletSpawners)
        {
            spawner.StartEndlessShooting();
        }
        BafflePlate.SetVisibility(false);
        rightBarrier.SetActive(false);
        currentDialogueText = InvincibleDashPrompt;
    }
    private void EndInvincibleDash()
    {
        foreach (BulletSpawner spawner in bulletSpawners)
        {
            spawner.isShooting = false;
        }
        BafflePlate.SetVisibility(true);
    }
    public void RegisterBulletSpawner(BulletSpawner spawner)
    {
        bulletSpawners.Add(spawner);
    }
    public void OnDialogueEnd()
    {
        switch(state)
        {
            case DialogueState.ClusterBombTutorial:
                currentDialogueText = PickupPrompt;
                Instantiate(ClusterBomb, AbilityPos.position, Quaternion.identity);
                break;
            case DialogueState.OnClusterBombPickup:
                currentDialogueText = ClusterBombTriggerPrompt;
                enemyManager.SpawnEnemies();
                break;
            case DialogueState.InvincibleDashTutorial:
                currentDialogueText= PickupPrompt;
                Instantiate(InvincibleDash, AbilityPos.position, Quaternion.identity);
                break;
            case DialogueState.OnInvincibleDashPickup:
                currentDialogueText = ReadyToDash;
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
