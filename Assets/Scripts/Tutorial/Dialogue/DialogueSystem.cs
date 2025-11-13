using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonParent;

    //[NonSerialized] GameObject dialogFrame;
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private float displayInterval = 0.1f;
    [SerializeField] private TextMeshProUGUI displayText;
    //[NonSerialized] int index;
    [NonSerialized] private bool isDisplaying = false;
    [NonSerialized] private DialogueNode currentNode;
    //[NonSerialized] private List<string> textList=new List<string>();
    [NonSerialized] private Dictionary<string, int> idToIndex;
    private bool hasShownButton = false;

    public delegate void DialogueEndDelegate();
    public event DialogueEndDelegate OnDialogueEnd;



    private void OnEnable()
    {
        Time.timeScale = 0.0f;
    }
    private void OnDisable()
    {
        Time.timeScale = 1.0f;
    }
    public void SetDialogueText(DialogueText InDialogueText)
    {
        if (InDialogueText!=dialogueText)
        {
            dialogueText = InDialogueText;
            Initialize();
        }
    }
    public void Initialize()
    {

        idToIndex = new Dictionary<string, int>();
        for (int i = 0; i < dialogueText.dialogueNodes.Count; i++)
        {
            DialogueNode node = dialogueText.dialogueNodes[i];
            if (!idToIndex.ContainsKey(node.id))
            {
                idToIndex[node.id] = i;
            }
            else
            {
                throw new System.Exception($"repetitive DialogueNode id: {node.id}!");
            }
        }
    }
    public DialogueNode GetNodeById(string id)
    {
        int index;
        if (idToIndex.TryGetValue(id, out index))
        {
            return dialogueText.dialogueNodes[index];
        }
        else
        {
            return null;
        }
    }
    //private void Awake()
    //{

    //}
    //private void OnEnable()
    //{
    //    index = 0;
    //    isDisplaying = true;
    //    StartCoroutine(DisplayTextCoroutine());

    //}
    //public void SetTextFromFile()
    //{
    //    textList.Clear();
    //    string[] texts = dialogueText.text.Split('\n');
    //    foreach (string text in texts)
    //    {
    //        textList.Add(text);
    //    }
    //}
    public void StartDialogue(string startId)
    {
        currentNode = GetNodeById(startId);
        ActivateDialogue();
    }
    public void ActivateDialogue()
    {
        if (!isDisplaying)
        {
            //if (index< dialogueText.textList.Count)
            //{
            //    isDisplaying = true;
            //    StartCoroutine(DisplayTextCoroutine());
            //}
            //else
            //{
            //    gameObject.SetActive(false);
            //}  
            if (currentNode != null)
            {
                if (!hasShownButton)
                {
                    isDisplaying = true;
                    StartCoroutine(DisplayTextCoroutine());
                }        
            }
            else
            {
                gameObject.SetActive(false);
                OnDialogueEnd?.Invoke();
            }
        }
        else
        {
            StopAllCoroutines();
            displayText.text = currentNode.text;
            PostProcess();
            isDisplaying =false;
        }

    }
    private IEnumerator DisplayTextCoroutine()
    {
        displayText.text = "";
        for (int i=0;i< currentNode.text.Length;i++)
        {
            displayText.text += currentNode.text[i];
            yield return new WaitForSecondsRealtime(displayInterval);
        }
        PostProcess();
        isDisplaying =false;
    }
    private void PostProcess()
    {
        switch (currentNode.nextNodeIds.Count)
        {
            //case 0:
            //    gameObject.SetActive(false);
            //    break;
            case 1:
                currentNode = GetNodeById(currentNode.nextNodeIds[0]);
                break;
            default:
                ShowButton();
                break;
        }
    }
    private void ShowButton()
    {
        
        for (int i=0;i< currentNode.choiceText.Count;i++)
        {
            int index = i; //close capture 
            Button button = Instantiate(buttonPrefab, buttonParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text=currentNode.choiceText[i];
            button.onClick.AddListener(() =>
            {
                DialogueEventManager.instance.Invoke(currentNode.choiceEventKeys[index]);
                currentNode = GetNodeById(currentNode.nextNodeIds[index]);
                hasShownButton = false;
                foreach (Transform child in buttonParent)
                {
                    Destroy(child.gameObject);
                }
                ActivateDialogue();
                
            });
        }
        hasShownButton = true;
    }

}
