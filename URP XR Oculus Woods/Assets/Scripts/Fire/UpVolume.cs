using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class increases the volume of an AudioSource over time. The script starts by getting the AudioSource component 
 * attached to the GameObject it's attached to and sets the initial volume to the AudioSource's current volume. 
 * Then it starts a coroutine that increases the volume over a specified amount of time using a while loop 
 * and WaitForSecondsRealtime.
 */
public class UpVolume : MonoBehaviour
{
    [SerializeField] float Time = 3;
    AudioSource audioSource;
    public float vol;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        vol = audioSource.volume;
        StartCoroutine(UpVolumeSeconds(Time));
    }

    private IEnumerator UpVolumeSeconds(float v)
    {
        float step = .05f;
        float timer = 0;
        while (timer < v)
        {
            timer += step;
            // When woodsSettings is converted, add this line again
            // audioSource.volume = vol * timer * WoodsSettings.Instance.Enviroment / v;
            audioSource.volume = (float)v;
            yield return new WaitForSecondsRealtime(step);
        }
    }
}
