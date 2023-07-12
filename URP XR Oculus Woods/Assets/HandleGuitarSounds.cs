﻿using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Content.Interaction;

public class HandleGuitarSounds : MonoBehaviour
{
    //[SerializeField] LinearMapping LinearMapping;
    [SerializeField] SoundData Sound;
    [SerializeField] Transform SoundBox;
    [SerializeField]
    public bool _playPressed;
    [SerializeField]
    public bool _chordPressed = false;
    public XRSlider handle;

    public GameObject grabPoint;
    public GameObject guitar;

    //[SerializeField]
    //private UnityEvent _whenSelect;
    //public UnityEvent WhenSelect => _whenSelect;

    int index = 0;
    private float _distance = 0;

    [SerializeField] float[] ChordsLenght;
    float ChordTotalLenght;

    private void Start()
    {

        _playPressed = false;
        gameObject.SetActive(false);

        // Calculate the total length of all chords.
        ChordTotalLenght = 0;
        foreach (var f in ChordsLenght)
        {
            ChordTotalLenght += f;
        }

        // Calculate the distance between the start and end positions of the linear mapping.
        float initvalue = handle.m_MaxPosition;
        float endValue = handle.m_MinPosition;
        _distance = initvalue - endValue;

    }
    public void chordPress()
    {
        if (_chordPressed)
            _chordPressed = false;
        else
            _chordPressed = true;
    }

    // Play the sound based on the linear mapping value and user input
    public void Play()
    {
        if (_chordPressed)
        {
            float value = calculateValue(handle.value);
            index = CalculateIndex(handle.value);
            index = (int)(Mathf.Clamp(index + 1, 0, Sound.Clips.Length - 1));
            SoundManager.Instance.PlayEffect(Sound.Clips[index], SoundBox, 1);
        }
        else
            SoundManager.Instance.PlayEffect(Sound.Clips[0], SoundBox, 1);

    }

    // Calculate the index of the chord to play, based on the position of the grab point in the handle
    int CalculateIndex(float value)
    {
        //value = 1 - value / _distance;
        float v = ChordTotalLenght * value;
        float sum = 0;

        for (int i = 0; i < ChordsLenght.Length; i++)
        {
            sum += ChordsLenght[i];
            if (sum >= v)
            {
                return i;
            }
        }

        return ChordsLenght.Length;
    }
    float calculateValue(float val)
    {
        float toRet = 0;
        Debug.LogError("Value: " + handle.value + " .Min: " + handle.m_MinPosition + " .Max: " + handle.m_MaxPosition);
        toRet = (val - handle.m_MinPosition) / (_distance);

        return toRet;
       
    }

}
