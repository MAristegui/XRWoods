using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataSlot
{
    public LogSaveData[] Logs;
    public int daysPassed = 1;
    public int[] LogsOutStack;
    public int SpecialIndex;

    public static float[] Vector3ToArray(Vector3 vector)
    {
        float[] array = new float[3];

        array[0] = vector.x;
        array[1] = vector.y;
        array[2] = vector.z;
        return array;
    }

    public static Vector3 ArrayToVector3(float[] array)
    {
        Vector3 vector = new Vector3(array[0], array[1], array[2]);

        return vector;
    }

    public static SerializedPlane[] PlanesToSerializedPlanes(EzySlice.Plane[] planes)
    {
        SerializedPlane[] ret = new SerializedPlane[planes.Length];

        for(int i =0; i< planes.Length; i++)
        {
            ret[i] = new SerializedPlane();
            ret[i].m_normal = Vector3ToArray(planes[i].m_normal);
            ret[i].m_dist = planes[i].m_dist;
        }


        return ret;
    }

    public static EzySlice.Plane[] SerializedPlanesToPlanes(SerializedPlane[] planes)
    {
        EzySlice.Plane[] ret = new EzySlice.Plane[planes.Length];

        for (int i = 0; i < planes.Length; i++)
        {
            ret[i] = new EzySlice.Plane(ArrayToVector3(planes[i].m_normal), planes[i].m_dist);
        }

        return ret;
    }

}

[System.Serializable]
public class LogSaveData
{
    public int Index;
    public SerializedPlane[] Cuts;
    public int[] Hulls;
    public float[] Position;
    public float[] Rotation;
    public float[] Scale;
    public bool Special;
}

[System.Serializable]
public class SerializedPlane
{
    public float[] m_normal;
    public float m_dist;
}

