using System;
using UnityEngine;

public class TutorialNPC : MonoBehaviour,IInteractable
{
    //public enum Room
    //{
    //    Room0,
    //    Room1, 
    //    Room2,
    //    Room3,
    //    Room4
    //}
    [NonSerialized] private PlayerController pc;
    [NonSerialized] private GameObject promptIcon;
    [SerializeField] private BaseRoom belongedRoom;
    [SerializeField] private DialogueSystem dialogSystem;

    private void Awake()
    {
        promptIcon = transform.Find("PromptIcon").gameObject;
    }
    private void Start()
    {
        pc = FindFirstObjectByType<PlayerController>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            pc.SetInteractObject(this);
            promptIcon.SetActive(true);

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (pc.GetInteractObject() == (IInteractable)this)
                pc.SetInteractObject(null);
            promptIcon.SetActive(false);
        }
    }

    public void Interact()
    {
        if (!dialogSystem.gameObject.activeSelf)
        {
            dialogSystem.gameObject.SetActive(true);
            dialogSystem.SetDialogueText(belongedRoom.GetCurrentDialogueText());
            dialogSystem.StartDialogue("Start");
        }
        else
        {
            dialogSystem.ActivateDialogue();
        }
    }
    private void OnDestroy()
    {
        if (pc.GetInteractObject() == (IInteractable)this)
            pc.SetInteractObject(null);
    }
}
