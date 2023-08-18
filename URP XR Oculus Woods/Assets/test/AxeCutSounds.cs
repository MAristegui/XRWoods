using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CutSound
{
    SoftHit,
    Hit,
    LittleCut,
    MidCut,
    BigCut,
    GrabAxe,
    GrabAxeStump,
}

[System.Serializable]
public struct CutSoundEntry
{
    [SerializeField] public CutSound Type;
    [SerializeField] public SoundData Data;
}

public class AxeCutSounds : MonoBehaviour
{
    [SerializeField] CutSoundEntry[] Entries;

    Dictionary<CutSound, SoundData> Dictionary;

    private void Start()
    {
        Dictionary = new Dictionary<CutSound, SoundData>();
        foreach(var e in Entries)
        {
            Dictionary.Add(e.Type, e.Data);
        }
    }

    public void PlaySound(CutSound sound, bool stopOthers)
    {
        //if (Flag) return;
        if (Dictionary.ContainsKey(sound))
        {
            if(stopOthers)
                SoundManager.Instance.StopAllSounds(tag);
            SoundManager.Instance.PlayEffect(Dictionary[sound].GetRandom(), transform, 1);
          //  Flag = true;
            //StartCoroutine(UnlockAfterSecond(0.05f));
        }

    }


}
