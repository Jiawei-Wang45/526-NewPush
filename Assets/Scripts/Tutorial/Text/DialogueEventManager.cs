using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueEventManager : MonoBehaviour
{
    public static DialogueEventManager instance;
    [NonSerialized] private Dictionary<string,Action> eventMap=new Dictionary<string,Action>();

    private void Awake()
    {
        instance = this;
    }
    public void RegisterEvent(string name, Action action)
    {
        if (!eventMap.ContainsKey(name))
        {
            eventMap.Add(name, action);
        }
    }
    public void Invoke(string name)
    {
        if (eventMap.TryGetValue(name, out Action action))
        {
            action?.Invoke();
        }
    }
}
