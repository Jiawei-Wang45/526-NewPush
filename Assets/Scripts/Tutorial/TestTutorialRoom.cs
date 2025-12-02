using System;
using UnityEngine;

public class TestTutorialRoom : BaseRoom
{
    public enum DialogueState
    {
        Introduction,
        None
    }

    [NonSerialized] private PlayerController pc;
    [NonSerialized] private DialogueState state = DialogueState.None;

    [Header("DialogueText")]
    [SerializeField] private DialogueText Introduction;
    [SerializeField] private DialogueText backToMainMenu;

    [Header("EnemyManager")]
    [SerializeField] private EnemyManager_Tutorial enemyManager;


    public override void Awake()
    {
        //leftBarrier.SetActive(false);
    }
    private void Start()
    {
        pc = FindFirstObjectByType<PlayerController>();
        dialogSystem.OnDialogueEnd += OnDialogueEnd;
        DialogueEventManager.instance.RegisterEvent("OnBack", BackToMainMenu);
    }
    public override void PlayerEntered()
    {
        NPC.gameObject.SetActive(true);
        pc.InteractObject = NPC;
        state = DialogueState.Introduction;
        dialogSystem.gameObject.SetActive(true);
        dialogSystem.SetDialogueText(Introduction);
        dialogSystem.StartDialogue("Start");
    }
    public void OnEnemyDestroyed(int index)
    {
        enemyManager.OnEnemyDestroyed(index);

    }
    public void OnDialogueEnd()
    {
        switch(state)
        {
            case DialogueState.Introduction:
                currentDialogueText = backToMainMenu;
                enemyManager.SpawnEnemies();
                FindFirstObjectByType<PlayerStats>().preventDamage = true;
                break;
        }


        if (state != DialogueState.None)
        {
            pc.InteractObject = null;
            state = DialogueState.None;
        }
    }
    public void BackToMainMenu()
    {
        GameManager.instance.BackToMainMenu();
    }
}
