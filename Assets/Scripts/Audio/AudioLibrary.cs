using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioLibrary", menuName ="AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    [System.Serializable]
    public class NameClipPair
    {
        public string name;
        public AudioClip clip;
    }

    public List<NameClipPair> clips;

}
