using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsForLog : MonoBehaviour
{
    public const int UpperHull = 0;
    public const int LowerHull = 1;
    public List<EzySlice.Plane> Cuts = new List<EzySlice.Plane>();
    public List<int> Hull = new List<int>();
    public List<int> indexes = new List<int>();
    static int index;
    public LogSaveData LogSaveData;
    
    public void AddCutPlane(EzySlice.Plane plane, int hull)
    {
        Cuts.Add(plane);
        Hull.Add(hull);
        indexes.Add(index);
        index++;
    }

    public LogSaveData UpdateSavedData()
    {
        if (LogSaveData == null)
            LogSaveData = new LogSaveData();

        LogSaveData.Hulls = Hull.ToArray();
        LogSaveData.Cuts = SaveDataSlot.PlanesToSerializedPlanes(Cuts.ToArray());
        LogSaveData.Position = SaveDataSlot.Vector3ToArray(transform.parent.position);
        LogSaveData.Rotation = SaveDataSlot.Vector3ToArray(transform.parent.rotation.eulerAngles);
        LogSaveData.Scale = SaveDataSlot.Vector3ToArray(transform.parent.localScale);
        return LogSaveData;

    }

    
}
