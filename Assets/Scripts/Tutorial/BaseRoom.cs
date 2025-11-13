using System;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    public enum TutorialRoom
    {
        DefenseTutorialRoom,
        AbilityTutorialRoom,
        TestTutorialRoom
    }
    [NonSerialized] protected DialogueText currentDialogueText = null;
    [SerializeField] protected DialogueSystem dialogSystem;
    //[SerializeField] protected DialogueText dialogueText;
    [SerializeField] protected TutorialNPC NPC;
    [SerializeField] protected GameObject leftBarrier;
    [SerializeField] protected GameObject rightBarrier;

    public virtual void Awake()
    {
        leftBarrier.SetActive(false);
        rightBarrier.SetActive(false);
    }
    public virtual void TrapPlayer()
    {
        leftBarrier.SetActive(true);
        rightBarrier.SetActive(true);
    }
    public virtual void ReleasePlayer()
    {
        rightBarrier.SetActive(false);
        Destroy(NPC.gameObject);
    }
    public virtual DialogueText GetCurrentDialogueText() 
    { 
        return currentDialogueText; 
    }

}
