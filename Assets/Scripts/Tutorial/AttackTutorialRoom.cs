using JetBrains.Annotations;
using System;
using UnityEngine;

public class AttackTutorialRoom : BaseRoom
{
    public enum DialogueState
    {
        SpawnPistol,
        SpawnFirstEnemy,
        SpawnSword,
        SpawnSecondEnemy,
        finished,
        None
    }


    [NonSerialized] private PlayerController pc;
    [NonSerialized] private Collider2D cd;
    [Header("DialogueText")]
    [SerializeField] private DialogueText EquipHint;
    [SerializeField] private DialogueText FiringHint;
    [SerializeField] private DialogueText SpawnPistol; 
    [SerializeField] private DialogueText FirstEnemySpawn;
    [SerializeField] private DialogueText SpawnSword;
    [SerializeField] private DialogueText SecondEnemySpawn;
    [SerializeField] private DialogueText EndDialogue;
    

    [Header("SpawnedItem")]
    [SerializeField] private Transform WeaponPos;
    [SerializeField] private PickupWeapon_Tutorial Pistol;
    [SerializeField] private PickupWeapon_Tutorial Sword;
    [SerializeField] private Transform EnemyPos;
    [SerializeField] private EnemySpawnIndicator_Tutorial firstSpawnIndicator;
    [SerializeField] private EnemySpawnIndicator_Tutorial secondSpawnIndicator;

    private DialogueState state=DialogueState.None;
    private void Start()
    {
        pc = FindFirstObjectByType<PlayerController>();
        cd=GetComponent<Collider2D>();
        dialogSystem.OnDialogueEnd += OnDialogueEnd;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer==LayerMask.NameToLayer("Player"))
        {
            TrapPlayer();
            NPC.gameObject.SetActive(true);
            pc.InteractObject = NPC;
            state = DialogueState.SpawnPistol;
            dialogSystem.gameObject.SetActive(true);
            dialogSystem.SetDialogueText(SpawnPistol);
            dialogSystem.StartDialogue("Start");
            cd.enabled = false;
        }
    }
    public void OnFirstWeaponPicked()
    {
        pc.InteractObject = NPC;
        state = DialogueState.SpawnFirstEnemy;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(FirstEnemySpawn);
        dialogSystem.StartDialogue("Start");
    }
    public void OnFirstEnemyKilled()
    {
        pc.InteractObject = NPC;
        state = DialogueState.SpawnSword;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(SpawnSword);
        dialogSystem.StartDialogue("Start");
    }
    public void OnSecondWeaponPicked()
    {
        pc.InteractObject = NPC;
        state = DialogueState.SpawnSecondEnemy;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(SecondEnemySpawn);
        dialogSystem.StartDialogue("Start");
    }
    public void OnSecondEnemyKilled()
    {
        pc.InteractObject = NPC;
        state = DialogueState.finished;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(EndDialogue);
        dialogSystem.StartDialogue("Start");
    }
    public void OnDialogueEnd()
    {
        switch(state)
        {
            case DialogueState.SpawnPistol:
                currentDialogueText = EquipHint;
                Instantiate(Pistol, WeaponPos.position, Quaternion.identity);
                break;
            case DialogueState.SpawnFirstEnemy:
                currentDialogueText = FiringHint;
                Instantiate(firstSpawnIndicator, EnemyPos.position, Quaternion.identity);
                break;
            case DialogueState.SpawnSword:
                currentDialogueText = EquipHint;
                Instantiate(Sword, WeaponPos.position, Quaternion.identity);
                break;
            case DialogueState.SpawnSecondEnemy:
                currentDialogueText = FiringHint;
                Instantiate(secondSpawnIndicator, EnemyPos.position, Quaternion.identity);
                break;
            case DialogueState.finished:
                ReleasePlayer();
                dialogSystem.OnDialogueEnd -= OnDialogueEnd;
                break;
        }
        if (state!= DialogueState.None)
        {
            pc.InteractObject = null;
            state = DialogueState.None;
        }
    }
}
