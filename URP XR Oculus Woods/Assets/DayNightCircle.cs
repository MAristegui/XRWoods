using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCircle : MonoBehaviour
{
    Vector3 rot = Vector3.zero;
    float degreesPerSecond = 6.0f;

    int nightStart = 30;
    int dayStart = 150;
    int afternoonStart = 270;

    int degreeAtNight = 6;
    int degreeAtDay = 10;
    int degreeAtAfternoon = 25;

    int limite;

    // Update is called once per frame
    void Update()
    {
 
        degreesPerSecond = calculateDegree(transform.localEulerAngles.x);
        //Debug.LogError(transform.localEulerAngles.x +" --- "+degreesPerSecond);
        rot.x = degreesPerSecond * Time.deltaTime;
        transform.Rotate(rot,Space.World);
    }

    int calculateDegree(float angle)
    {
        int degree = 0;

        if (angle>=nightStart && angle<dayStart)
            degree = degreeAtNight;
        else 
            if (angle>=dayStart && angle<afternoonStart)
                degree = degreeAtDay;
        else
            degree = degreeAtAfternoon;

        return degree;
    }
}
