using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshMath
{

    public static Vector3 MassCenter(Mesh mesh)
    {
        if (mesh.vertexCount == 0)
        {
            return Vector3.zero;
        }
        Vector3 firstPoint = mesh.vertices[0];
        float x_min = firstPoint.x,
            x_max = firstPoint.x,
            y_min = firstPoint.y,
            y_max = firstPoint.y,
            z_min = firstPoint.z,
            z_max = firstPoint.z;

        Vector3[] vertices = mesh.vertices;
        foreach (var v in vertices)
        {
            if (v.x > x_max)
                x_max = v.x;
            if (v.x < x_min)
                x_min = v.x;
            if (v.y > y_max)
                y_max = v.y;
            if (v.y < y_min)
                y_min = v.y;
            if (v.z > z_max)
                z_max = v.z;
            if (v.z < z_min)
                z_min = v.z;
        }

        return new Vector3(x_max + x_min, y_max + y_min, z_max + z_min) / 2;
    }

    public static float CalculateVolumeAndMassCenter(Mesh mesh, out Vector3 massCenter, out float Radius)
    {
        if (mesh.vertexCount == 0) 
        {
            massCenter = Vector3.zero;
            Radius = 0;
            return 0;
        }
        Vector3 firstPoint = mesh.vertices[0];
        float x_min = firstPoint.x,
            x_max = firstPoint.x,
            y_min = firstPoint.y,
            y_max = firstPoint.y,
            z_min = firstPoint.z,
            z_max = firstPoint.z;

        Vector3[] vertices = mesh.vertices;
        foreach(var v in vertices)
        {
            if (v.x > x_max)
                x_max = v.x;
            if (v.x < x_min)
                x_min = v.x;
            if (v.y > y_max)
                y_max = v.y;
            if (v.y < y_min)
                y_min = v.y; 
            if (v.z > z_max)
                z_max = v.z;
            if (v.z < z_min)
                z_min = v.z;
        }

        massCenter = new Vector3(x_max + x_min, y_max + y_min, z_max + z_min) / 2;

        Radius = x_max - x_min;

        return Mathf.Abs((x_max - x_min) * (y_max - y_min) * (z_max - z_min));

    }

    public static float CalculateVolume(Mesh mesh)
    {
        if (mesh.vertexCount == 0)
        {
            
            return 0;
        }
        Vector3 firstPoint = mesh.vertices[0];
        float x_min = firstPoint.x,
            x_max = firstPoint.x,
            y_min = firstPoint.y,
            y_max = firstPoint.y,
            z_min = firstPoint.z,
            z_max = firstPoint.z;

        Vector3[] vertices = mesh.vertices;
        foreach (var v in vertices)
        {
            if (v.x > x_max)
                x_max = v.x;
            if (v.x < x_min)
                x_min = v.x;
            if (v.y > y_max)
                y_max = v.y;
            if (v.y < y_min)
                y_min = v.y;
            if (v.z > z_max)
                z_max = v.z;
            if (v.z < z_min)
                z_min = v.z;
        }

        return Mathf.Abs((x_max - x_min) * (y_max - y_min) * (z_max - z_min));

    }

}
