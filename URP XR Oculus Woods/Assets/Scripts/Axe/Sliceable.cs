using EzySlice;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Receiver.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Rendering;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.State;
using UnityEngine.XR.Interaction.Toolkit.AffordanceSystem.Theme.Primitives;


public class Sliceable : MonoBehaviour
{
    public Transform startSlicePoint;
    public Transform endSlicePoint;
    public VelocityEstimator velocityEstimator;
    public float cutForce = 0.0f;
    public LayerMask sliceableLayer;
    public float minVelocity = 1.0f;
    [SerializeField]
    private Material chopMaterial;


    private float _nextCut = 5.0f;
    private float _cutDelay = 5.0f;

    public ColorAffordanceThemeDatumProperty outlineColor;

    // Start is called before the first frame update
    void Start()
    {
        if (chopMaterial==null)
            chopMaterial = Resources.Load<Material>("\\Models\\Materials\\Logs\\Texture\\Inside");
        Debug.LogError("OnStart: " + chopMaterial.name);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        bool hasHit = Physics.Linecast(startSlicePoint.position, endSlicePoint.position, out RaycastHit hit, sliceableLayer);

        if (hasHit && Time.time > _nextCut)
        {
            GameObject target = hit.transform.gameObject;
            //Debug.LogError("gameobject: " + target.name);
            Slice(target, chopMaterial);
            //_nextCut += _cutDelay;
            _nextCut = Time.time + _cutDelay;
        }
    }

    public void Slice(GameObject target, Material insideMaterial)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        //if (velocity.magnitude < minVelocity) return;

        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        UVOffset uvoffset = new UVOffset();
        uvoffset.offset = new Vector2(-0.6f, 0.335f);
        uvoffset.scale = Vector2.one * 0.298f;
        uvoffset.value = 1;

        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal, ref uvoffset, insideMaterial);
        Debug.LogError(insideMaterial);
        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target);
            if (checkHull(upperHull)) return;

            SetupSlicedComponent(upperHull, target);
            Vector3 secondPlaneNormal = Quaternion.AngleAxis(-45, Vector3.up) * planeNormal;
            secondPlaneNormal.Normalize();

            SlicedHull hull2 = upperHull.Slice(endSlicePoint.position, secondPlaneNormal, ref uvoffset, insideMaterial);

            Debug.LogError(hull2 == null);
            if (hull2 != null)
            {

                GameObject secondUpper = hull2.CreateUpperHull(upperHull);
                if (checkHull(secondUpper)) return;
                SetupSlicedComponent(secondUpper, target);

                //GameObject secondLower = hull2.CreateLowerHull(upperHull);
                //SetupSlicedComponent(secondLower, target);
                //Destroy(secondLower);

                //Revisar la siguiente linea, capas no va aca
                Destroy(upperHull);

            }

            GameObject lowerHull = hull.CreateLowerHull(target);
            if (checkHull(lowerHull)) return;
            SetupSlicedComponent(lowerHull, target);

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
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = vertices[i] + center;
        }
        mesh.vertices = vertices;
        slicedObject.transform.position -= center;

        meshFilter.sharedMesh = mesh;
        collider.sharedMesh = mesh;

        //slicedObject.AddComponent<DrawRendererBounds>();


        GameObject fbe = new GameObject("FeedbackEffects");
        fbe.transform.parent = slicedObject.transform;
        XRInteractableAffordanceStateProvider provider = fbe.AddComponent<XRInteractableAffordanceStateProvider>();
        provider.interactableSource = grab;
        provider.activateClickAnimationMode = XRInteractableAffordanceStateProvider.ActivateClickAnimationMode.Activated;

        /*GameObject ve = new GameObject("VisualEffects");
        ve.transform.parent = fbe.transform;
        MaterialPropertyBlockHelper mpbe = ve.AddComponent<MaterialPropertyBlockHelper>();
        mpbe.rendererTarget = slicedObject.GetComponent<Renderer>();
        ColorMaterialPropertyAffordanceReceiver cmpa = ve.AddComponent<ColorMaterialPropertyAffordanceReceiver>();
        cmpa.affordanceStateProvider = provider;
        //cmpa.affordanceThemeDatum = outlineColor;

        if (original.GetComponentInChildren<ColorMaterialPropertyAffordanceReceiver>())
        {
            cmpa.affordanceThemeDatum = original.GetComponentInChildren<ColorMaterialPropertyAffordanceReceiver>().affordanceThemeDatum;
            cmpa.replaceIdleStateValueWithInitialValue = true;
        }*/

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
            //cmpa.affordanceThemeDatum = outlineColor;

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
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(startSlicePoint.position, endSlicePoint.position);
    }
}
