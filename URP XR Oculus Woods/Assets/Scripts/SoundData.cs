using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffectData", menuName = "Woods/Sound Effect Data", order = 51)]

public class SoundData : ScriptableObject
{
    public AudioClip[] Clips;

    internal AudioClip GetRandom()
    {
        float value = UnityEngine.Random.value;
        int index = Mathf.RoundToInt(value * Clips.Length);
        if (index < Clips.Length)
            return Clips[index];
        else
            return Clips[0];
    }
}
