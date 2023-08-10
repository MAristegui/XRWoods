using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (!_instance) _instance = FindObjectOfType<SoundManager>();

            return _instance;
        }
    }

    public float Volume;

    [SerializeField] AudioSource[] AudioSources;

    int _index = 0;

    private void Awake()
    {
        _instance = this;
        AudioSources = GetComponentsInChildren<AudioSource>();
    }

    public void StopAllSounds(string tag)
    {
        foreach (var a in AudioSources)
        {
            if (a.tag == tag)
                a.Stop();
        }
    }

    public AudioSource PlayEffect(AudioClip clip, Transform emmiter, float pitch_value)
    {
        AudioSource audioSource = AudioSources[_index];

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.transform.position = emmiter.position;
        audioSource.tag = emmiter.tag;
        audioSource.pitch = pitch_value;
        audioSource.Play();

        _index = (_index + 1) % AudioSources.Length;

        return audioSource;
    }

    internal void ChangeEffectsVolume(float value)
    {
        Volume = value;
        foreach (var a in AudioSources)
        {
            a.volume = value;
        }

        //WoodsPlayer.Player.SnapTurn.snapTurnSource.volume = value;
        //GameManager.Instance.ChangeEffectVolume(value);
    }
}
