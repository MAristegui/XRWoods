using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using UnityEngine.InputSystem;

public class Sliceable : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public float cutForce = 0.0f;
    public LayerMask sliceableLayer;
    public float minVelocity = 1.0f;


    private float _nextCut = 1.0f;
    private float _cutDelay = 1.0f;

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
            SetupSlicedComponent(upperHull);
            Vector3 secondPlaneNormal = Quaternion.AngleAxis(-10, Vector3.up) * planeNormal;
            secondPlaneNormal.Normalize();
            SlicedHull hull2 = upperHull.Slice(endSlicePoint.position, secondPlaneNormal);
            if (hull2 != null)
            {
                GameObject secondUpper = hull2.CreateUpperHull(upperHull, upperHull.GetComponent<Renderer>().material);
                SetupSlicedComponent(secondUpper);
                
                GameObject secondLower = hull2.CreateLowerHull(upperHull, upperHull.GetComponent<Renderer>().material);
                SetupSlicedComponent(secondLower);
                Destroy(secondLower);

                //Revisar la siguiente linea, capas no va aca
                Destroy(upperHull);
            }

            GameObject lowerHull = hull.CreateLowerHull(target,target.GetComponent<Renderer>().material);
            SetupSlicedComponent(lowerHull);

            Destroy(target);
        }
    }

    public void SetupSlicedComponent(GameObject slicedObject)
    {
        
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;
        //rb.AddExplosionForce(cutForce, slicedObject.transform.position, 1);
        slicedObject.layer = LayerMask.NameToLayer("Sliceable");
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startSlicePoint.position, endSlicePoint.position);
    }
}
