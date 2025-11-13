using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

//[Serializable]
//public class DialogueChoiceEvent : UnityEvent { }


[Serializable]
public class DialogueNode
{
    public string id; //must unique
    public string text;
    //different number means differently, =1 means linear text, >1 means branch options, use null to stand for ending
    public List<string> nextNodeIds;
    //branch text in button, only exists when it has branch
    public List<string> choiceText;
    public List<string> choiceEventKeys;
}
// contains all dialogues with one talking
[CreateAssetMenu(fileName = "New Text", menuName = "Dialogue/DialogueText")]
public class DialogueText : ScriptableObject
{
    [SerializeField] public List<DialogueNode> dialogueNodes;
}
