using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogFragment : MonoBehaviour
{
    [Header("References")]
    public Log Parent;
    public CutsForLog CutsForLog;
    public MeshFilter MeshFilter;
    public MeshRenderer MeshRenderer;
    public MeshCollider Collider;
    public MeshCollider Trigger;
    public NodeLogFragment Node;

    [Header("Parameters")]
    public float Volume;

    public Vector3 MassCenter;

    public float Radius;

    [Header("Flags")]
    public bool Mark = false;
    private void Awake()
    {
        Trigger.sharedMesh = MeshFilter.mesh;
    }

    public void Init(Log parent)
    {
        Parent = parent;
        Volume = MeshMath.CalculateVolumeAndMassCenter(MeshFilter.mesh, out MassCenter,out Radius);
        Volume *= Mathf.Abs(Parent.transform.localScale.x);
    }
    public void InitAfterInstantiate(Log parent, Mesh mesh)
    {
        Parent = parent;
        Volume = MeshMath.CalculateVolumeAndMassCenter(mesh, out MassCenter, out Radius);
        Volume *= Mathf.Abs(Parent.transform.localScale.x);
        MeshFilter.mesh = mesh;
        Collider.sharedMesh = mesh;
        Trigger.sharedMesh = mesh;
    }

    public Vector3 CenterFragment(Vector3 MassCenter)
    {
        Mesh mesh = MeshFilter.mesh;

        Vector3[] vertices = mesh.vertices;

        for(int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += (MassCenter)-this.MassCenter;
        }

        mesh.vertices = vertices;

        mesh.vertices = vertices;
        MeshFilter.mesh = mesh; 
        MeshFilter.sharedMesh = mesh;

        Collider.sharedMesh = mesh;
        Trigger.sharedMesh = mesh;

        if (Parent)
            Parent.MassCenter = MassCenter;

        return (MassCenter) - this.MassCenter;
    }

    public void GenerateFragment()
    {
        Mesh mesh = MeshFilter.mesh;
        
        for (int i=0; i<CutsForLog.Cuts.Count; i++)
        {
            EzySlice.Plane plane = CutsForLog.Cuts[i];
            int hull = CutsForLog.Hull[i];
            
            Volume = MeshMath.CalculateVolumeAndMassCenter(mesh, out MassCenter, out Radius);

            Material material = GetComponent<MeshRenderer>().material;
            UVOffset uvoffset = new UVOffset();
            uvoffset.offset = Parent.UVOffset;
            uvoffset.scale = Parent.UVScale;
            uvoffset.value = Radius;

            SlicedHull result = gameObject.Slice(plane, ref uvoffset, material);
            if (result == null)
            {
                Debug.LogError("Error Slicing LogFragment");
                
            }
            GameObject loghull = null;
            if (hull == CutsForLog.UpperHull)
            {
                if (result != null)
                    loghull = result.CreateUpperHull(gameObject, material);
            }
            if (hull == CutsForLog.LowerHull)
            {
                if (result != null)
                    loghull = result.CreateLowerHull(gameObject, material);
            }

            if (loghull)
            {
                loghull.SetActive(false);
                if (loghull.GetComponent<MeshFilter>().mesh)
                {
                    
                    InitAfterInstantiate(Parent, loghull.GetComponent<MeshFilter>().mesh);
                    Parent.RecalculateMass();
                    //CenterFragment(Parent.MassCenter);
                    transform.localPosition = Vector3.zero;
                    transform.localScale = Vector3.one;
                }
                else
                {
                    Debug.LogError("Error Slicing LogFragment - Mesh");
                }
                Destroy(loghull);
            }
            else
            {
                Destroy(loghull);
                Debug.LogError("Error Slicing LogFragment - Hull");
            }
        }

        CenterFragment(Parent.MassCenter);
    }

    public LogSaveData GetSaveData()
    {
        LogSaveData data = CutsForLog.UpdateSavedData();
        data.Special = Parent.Special;
        data.Index = Parent.Index;

        return data;
    }
}
