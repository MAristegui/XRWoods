using EzySlice;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;


public class Sliceable : MonoBehaviour
{
    // Slice atributes
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public LayerMask sliceableLayer;
    [SerializeField]
    private Material chopMaterial;
    public ColorAffordanceThemeDatumProperty outlineColor;

    [SerializeField]
    public Transform cutPlane1;
    [SerializeField]
    public Transform cutPlane2;

    public float minVelocity = 1.5f;
    public float cutForce = 0.2f;

    // Timer delay 
    private float _nextCut = 0.0f;
    private float _cutDelay = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        if (chopMaterial == null)
            chopMaterial = Resources.Load<Material>("\\Models\\Materials\\Logs\\Texture\\Inside");
       
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);

        if (hasHit && Time.time > _nextCut)
        {
            GameObject target = hit.transform.gameObject;
            bool sliced = Slice(target, chopMaterial);

            // Avoid allowing more than one cut simultaneusly (Time.time >> _nextCut)
            //_nextCut += _cutDelay;
            if (sliced) 
                _nextCut = Time.time + _cutDelay;
        }
    }

    public bool Slice(GameObject target, Material insideMaterial)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        Debug.Log(velocity.magnitude);
        //if (velocity.magnitude < minVelocity) return false;

        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        UVOffset uvoffset = new UVOffset();
        uvoffset.offset = new Vector2(-0.6f, 0.335f);
        uvoffset.scale = Vector2.one * 0.298f;
        uvoffset.value = 1;

        planeNormal = cutPlane1.transform.up;
        planeNormal.Normalize();
       
        Debug.LogError(planeNormal);
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal, ref uvoffset, insideMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, insideMaterial);
            if (checkHull(upperHull)) 
                return false;

            SetupSlicedComponent(upperHull, target);

            Vector3 secondPlaneNormal = Quaternion.AngleAxis(-5, Vector3.up) * planeNormal;
            secondPlaneNormal.Normalize();

           /*SlicedHull hull2 = upperHull.Slice(endSlicePoint.position, secondPlaneNormal, ref uvoffset, insideMaterial);

            if (hull2 != null)
            {

                GameObject secondUpper = hull2.CreateUpperHull(upperHull, insideMaterial);
                if (checkHull(secondUpper)) 
                    return false;
                SetupSlicedComponent(secondUpper, target);

                //GameObject secondLower = hull2.CreateLowerHull(upperHull);
                //SetupSlicedComponent(secondLower, target);
                //Destroy(secondLower);

                //Revisar la siguiente linea, capas no va aca
                Destroy(upperHull);

            }*/

            GameObject lowerHull = hull.CreateLowerHull(target, insideMaterial);
            if (checkHull(lowerHull))
                return false;
            SetupSlicedComponent(lowerHull, target);
            planeNormal = cutPlane2.transform.up;
            planeNormal.Normalize();
            planeNormal = -1 * planeNormal;
            SlicedHull hull2 = target.Slice(endSlicePoint.position, planeNormal, ref uvoffset, insideMaterial);
            if (hull2 != null) 
            {
                GameObject lowerHull2 = hull2.CreateLowerHull(target, insideMaterial);
                if (checkHull(lowerHull2)) return false;
                SetupSlicedComponent(lowerHull2, target);
         
            }
            Destroy(lowerHull);
            Destroy(target);

        }
        return true;
    }

    public void SetupSlicedComponent(GameObject slicedObject, GameObject original)
    {
        // Adding mesh atributes
        var meshFilter = slicedObject.GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;
        MeshCollider collider = slicedObject.AddComponent<MeshCollider>();
        collider.convex = true;

        var center = -MeshMath.MassCenter(collider.sharedMesh);
        var vertices = collider.sharedMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] + center;
        }
        mesh.vertices = vertices;
        slicedObject.transform.position -= center;

        meshFilter.sharedMesh = mesh;
        collider.sharedMesh = mesh;

        // Configuring base scripts of the sliced object
        slicedObject.layer = original.layer;
        Rigidbody rb = slicedObject.AddComponent<Rigidbody>();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.mass = 5.0f;
        XRGrabExt grab = slicedObject.AddComponent<XRGrabExt>();
        grab.attachTransform = slicedObject.transform;
        slicedObject.AddComponent<RayAttachModifier>();
        grab.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Kinematic;   

        // Outline in sliced objects
        GameObject fbe = new GameObject("FeedbackEffects");
        fbe.transform.parent = slicedObject.transform;
        XRInteractableAffordanceStateProvider provider = fbe.AddComponent<XRInteractableAffordanceStateProvider>();
        provider.interactableSource = grab;
        provider.activateClickAnimationMode = XRInteractableAffordanceStateProvider.ActivateClickAnimationMode.Activated;
        provider.ignoreActivateEvents = true;
        //provider.ignoreHoverEvents = true;
        provider.ignoreSelectEvents = true;
        // Allow outline in all materials of the sliced objects
        int index = 0;
        foreach (Material m in slicedObject.GetComponent<MeshRenderer>().materials)
        {
            GameObject ve = new GameObject("VisualEffects");
            ve.transform.parent = fbe.transform;
            MaterialPropertyBlockHelper mpbe = ve.AddComponent<MaterialPropertyBlockHelper>();
            mpbe.rendererTarget = slicedObject.GetComponent<Renderer>();
            mpbe.materialIndex = index;
            ColorMaterialPropertyAffordanceReceiver cmpa = ve.AddComponent<ColorMaterialPropertyAffordanceReceiver>();
            cmpa.affordanceStateProvider = provider;

            if (original.GetComponentInChildren<ColorMaterialPropertyAffordanceReceiver>())
            {
                cmpa.affordanceThemeDatum = original.GetComponentInChildren<ColorMaterialPropertyAffordanceReceiver>().affordanceThemeDatum;
                cmpa.replaceIdleStateValueWithInitialValue = true;
            }

            index++;
        }
    }

    private bool checkHull(GameObject hull)
    {
        if (hull == null)
        {
            Destroy(hull);
            return true;
        }
        if (hull.GetComponent<MeshFilter>() == null)
        {
            Destroy(hull);
            return true;
        }
        return false;
    }


    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startSlicePoint.position, endSlicePoint.position);
    }*/
}
