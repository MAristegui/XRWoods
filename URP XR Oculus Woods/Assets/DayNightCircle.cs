using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCircle : MonoBehaviour
{
    Vector3 rot = Vector3.zero;
    float degreesPerSecond = 6.0f;

    // Update is called once per frame
    void Update()
    {
        rot.x = degreesPerSecond * Time.deltaTime;
        transform.Rotate(rot,Space.World);
    }
}
