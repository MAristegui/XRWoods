using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogDispawn : MonoBehaviour
{
    Log Log;
    float TimeForDispawn = 300;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        Log = GetComponent<Log>();
    }

    // Update is called once per frame
    void Update()
    {
        if (
            !Log._destroy
            && !Log.Stacked
            && !Log.Special
           // && !Log.Interactable.attachedToHand
            && !Log.OnShed
            && !Log.Locked
            )
            timer += Time.deltaTime;
        else ResetTimer();


        if (timer > TimeForDispawn)
            Destroy(this.gameObject);
    }

    public void ResetTimer()
    {
        timer = 0;
    }

}
