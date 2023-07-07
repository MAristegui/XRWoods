using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;
using UnityEngine.XR.Content.Interaction;


public class Sliceable : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public float cutForce = 0.0f;
    public LayerMask sliceableLayer;
    public float minVelocity = 1.0f;


    private float _nextCut = 5.0f;
    private float _cutDelay = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);
        
        if (hasHit && Time.time > _nextCut)
        {
            
            GameObject target = hit.transform.gameObject;
            Debug.LogError("gameobject: " + target.name);
            Slice(target);
            _nextCut += _cutDelay;
        }
    }

    public void Slice(GameObject target)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        //if (velocity.magnitude < minVelocity) return;

        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target,target.GetComponent<Renderer>().material);
            SetupSlicedComponent(upperHull,target);
            Vector3 secondPlaneNormal = Quaternion.AngleAxis(-45, Vector3.up) * planeNormal;
            secondPlaneNormal.Normalize();
            SlicedHull hull2 = upperHull.Slice(endSlicePoint.position, secondPlaneNormal);
            if (hull2 != null)
            {
                
                GameObject secondUpper = hull2.CreateUpperHull(upperHull, upperHull.GetComponent<Renderer>().material);
                SetupSlicedComponent(secondUpper, target);
                
                GameObject secondLower = hull2.CreateLowerHull(upperHull, upperHull.GetComponent<Renderer>().material);
                //SetupSlicedComponent(secondLower, target);
                Destroy(secondLower);

                //Revisar la siguiente linea, capas no va aca
                Destroy(upperHull);

            }

            GameObject lowerHull = hull.CreateLowerHull(target,target.GetComponent<Renderer>().material);
            SetupSlicedComponent(lowerHull,target);
            
            Destroy(target);
        }
    }

    public void SetupSlicedComponent(GameObject slicedObject, GameObject original)
    {
        var meshFilter = slicedObject.GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;

        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        //rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
        slicedObject.layer = original.layer;
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        XRGrabExt grab = slicedObject.AddComponent<XRGrabExt>();
        grab.attachTransform = slicedObject.transform;
        slicedObject.AddComponent<RayAttachModifier>();
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Kinematic;

        var center = -MeshMath.MassCenter(collider.sharedMesh);
        var vertices = collider.sharedMesh.vertices;
        for (int i=0;i<vertices.Length;i++)
        {
            vertices[i] = vertices[i] + center;
        }
        mesh.vertices = vertices;
        slicedObject.transform.position -= center;

        meshFilter.sharedMesh = mesh;
        collider.sharedMesh = mesh;

        slicedObject.AddComponent<DrawRendererBounds>();

        var originalOutline = original.GetComponent<Outline>();
        Debug.LogError("Outline: " + originalOutline);
        if (originalOutline !=  null)
        {
            var outline = slicedObject.AddComponent<Outline>();
            //originalOutline.copy(outline);
            //outline.enabled = false;

            //grab.hoverEntered.AddListener((grab) => { outline.enabled = true; });
            //grab.hoverExited.AddListener((grab) => { outline.enabled = false; });

            //grab.hoverEntered = original.GetComponent<XRGrabExt>().hoverEntered;
           // grab.hoverExited = original.GetComponent<XRGrabExt>().hoverExited;


        }
        
        

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startSlicePoint.position, endSlicePoint.position);
    }
}
