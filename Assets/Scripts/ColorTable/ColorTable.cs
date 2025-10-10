using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorTable", menuName = "Config/ColorTable")]
public class ColorTable : ScriptableObject
{
    [System.Serializable]
    public struct Counter
    {
        public string counterColor;
        public float counterMultiplier;
    }




    [System.Serializable]
    public struct Entry
    {
        public string name;
        public Color color;
        public Counter[] counters;
    }

    public Entry[] entries;
    private Dictionary<string, Color> dictionary;
    private Dictionary<string, Dictionary<string, float>> counterDictionary;

    private void Init()
    {
        dictionary = new Dictionary<string, Color>();
        counterDictionary = new Dictionary<string, Dictionary<string, float>>();
        foreach (var entry in entries)
        {
            dictionary[entry.name] = entry.color;

            Dictionary<string, float> inner=new Dictionary<string, float>();
            if (entry.counters!=null)
            {
                foreach (var counter in entry.counters)
                {
                    inner[counter.counterColor]=counter.counterMultiplier;
                }
            }
            counterDictionary[entry.name]=inner;
        }
    }
    public Color GetColorByName(string name)
    {
        if (dictionary == null) 
            Init();
        if (dictionary.ContainsKey(name))
            return dictionary[name];
        Debug.Log("This name doesn't match any Color, check settings or typo");
        return Color.white;
    }
    public float GetMultiplier(string name,string counterName)
    {
        if (counterDictionary == null)
            Init();
        if (counterDictionary.TryGetValue(name,out var inner))
        {
            if (inner.TryGetValue(counterName,out float multiplier))
            {
                return multiplier;
            }
        }
        return 1.0f;
    }
}
