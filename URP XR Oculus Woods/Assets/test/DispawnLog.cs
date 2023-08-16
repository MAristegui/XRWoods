using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispawnLog : MonoBehaviour
{
    [SerializeField] Log Log;
    [SerializeField] float DispawnAfter = 1.5f;

    [SerializeField] ParticleSystem Ashes;

    bool _dispawn = false;

    

    private void Start()
    {
        // Falta woods.terrain
        //Transform Terrain = WoodsTerrain.Instance.transform;
       // Ashes.collision.SetPlane(0, Terrain); ;

    }

    public void Setup()
    {
        StartCoroutine(SetupAshes());
    }

    public void Dispawn()
    {
        if (_dispawn) return;
        _dispawn = true;
        float t = Ashes.main.startLifetime.constant;
        Ashes.Play();
        StartCoroutine(DispawnAfterSeconds(DispawnAfter));
        StartCoroutine(DestroyAfterSeconds(t));

    }

    IEnumerator SetupAshes()
    {
        yield return null;
        GenerateMesh();
        
    }

    IEnumerator DispawnAfterSeconds(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        {
            foreach (var f in Log.GetFragmentList())
            {
                f.MeshRenderer.enabled = false;
                f.Collider.enabled = false;
            }
        }
        
    }

    IEnumerator DestroyAfterSeconds(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        Destroy(gameObject);
    }

    void GenerateMesh()
    {
        List<LogFragment> list = Log.GetFragmentList();

        CombineInstance[] combine = new CombineInstance[list.Count];

        int i = 0;

        while (i < list.Count)
        {
            combine[i].mesh = list[i].MeshFilter.sharedMesh;
            combine[i].transform = Matrix4x4.identity;

            i++;
        }
        //Mesh mesh = new Mesh();
        

        var shape = Ashes.shape;
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);
        shape.mesh = mesh;

    }
}
