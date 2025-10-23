using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("Audio Library")]
    public AudioLibrary library;

    private AudioSource audioSource;
    private Dictionary<string,AudioClip> clipDict= new Dictionary<string,AudioClip>();
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        audioSource= GetComponent<AudioSource>();
        foreach(AudioLibrary.NameClipPair entry in library.clips)
        {
            if (entry!=null && !clipDict.ContainsKey(entry.name))
            {
                clipDict[entry.name] = entry.clip;
            }
        }
    }
    public void PlaySound(string name)
    {
        if (clipDict.TryGetValue(name,out AudioClip clip))
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
