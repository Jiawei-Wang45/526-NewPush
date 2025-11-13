using System;
using UnityEngine;

public class MoveTutorialRoom : BaseRoom
{
    [SerializeField] private DialogueText firstDialogueText;
    [SerializeField] private DialogueText secondDialogueText;
    private bool hasTalked = false;
    public override void Awake()
    {
        TrapPlayer();
    }
    private void Start()
    {
        DialogueEventManager.instance.RegisterEvent("OnPlayerReleased", ReleasePlayer);
    }
    public override DialogueText GetCurrentDialogueText()
    {
        if (!hasTalked)
        {
            hasTalked = true;
            return firstDialogueText;
        }
        else
        {
            return secondDialogueText;
        }
    }

}
