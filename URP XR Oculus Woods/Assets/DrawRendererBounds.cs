using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawRendererBounds : MonoBehaviour
{
    /*
    public void OnDrawGizmos()
    {
        var r = GetComponent<Renderer>();
        if (r == null)
            return;
        var bounds = r.bounds;
        Gizmos.matrix = Matrix4x4.identity;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(bounds.center, bounds.extents * 2);
        Gizmos.DrawSphere(transform.TransformPoint(Vector3.zero),radius:0.3f);
        //Gizmos.DrawMesh(GetComponent<Mesh>());
    }*/
}