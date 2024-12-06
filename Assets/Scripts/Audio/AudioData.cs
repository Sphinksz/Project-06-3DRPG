using System;
using UnityEngine;

[Serializable]
public struct AudioData 
{
    public string Name;
    public AudioClip Clip;
        
    public AudioData(string name, AudioClip clip)
    {
        Name = name;
        Clip = clip;
    }
}