using EzySlice;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Content.Interaction;
using UnityEngine.XR.Interaction.Toolkit;
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

    bool canCut = true;

    // From AxeCutter
    [SerializeField] AxeCollisionPoint CollisionPoint;
    [SerializeField] AxeCutPlanes Planes;
    [SerializeField] float _maxVolume = 0.01f;
    [SerializeField] float _minVolume = 0.001f;
    [SerializeField] float ForceMagnitude = 500;
    bool generateNew = false;
    int index = 0;
    bool cutting = false;
    bool f = false;
    float timer = 0;
    [SerializeField] float CheckTime = 0.3f;
    [SerializeField] float CheckTime2 = 0.01f;

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
        //Debug.LogError("HasHit: " + hasHit + " . Cancut: " + canCut);
        if (hasHit && canCut )//&& Time.time > _nextCut)
        {
            //Debug.LogError("Entro");
            canCut = false;
            
            GameObject target = hit.transform.gameObject;
            LogFragment toCut = target.GetComponentInChildren<LogFragment>();
            //bool sliced = Slice(target, chopMaterial);

            if (toCut)
                NormalCut(toCut);
           
            canCut = true;
        }

    }

    public bool Slice(GameObject target, Material insideMaterial)
    {
        Vector3 velocity = velocityEstimator.GetVelocityEstimate();
        //Debug.Log(velocity.magnitude);
        //if (velocity.magnitude < minVelocity) return false;

        Vector3 planeNormal = Vector3.Cross(endSlicePoint.position - startSlicePoint.position, velocity);
        planeNormal.Normalize();

        UVOffset uvoffset = new UVOffset();
        uvoffset.offset = new Vector2(-0.6f, 0.335f);
        uvoffset.scale = Vector2.one * 0.298f;
        uvoffset.value = 1;

        planeNormal = cutPlane1.transform.up;
        planeNormal.Normalize();

        // Debug.LogError(planeNormal);
        SlicedHull hull = target.Slice(endSlicePoint.position, planeNormal, ref uvoffset, insideMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(target, insideMaterial);
            if (checkHull(upperHull))
                return false;

            SetupSlicedComponent(upperHull, target);

            Vector3 secondPlaneNormal = Quaternion.AngleAxis(-5, Vector3.up) * planeNormal;
            secondPlaneNormal.Normalize();


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


    void NormalCut(LogFragment log)
    {

        Debug.LogError(log);
        Material material = log.GetComponent<MeshRenderer>().material;

        UVOffset uvoffset = new UVOffset();
        uvoffset.offset = log.Parent.UVOffset;
        uvoffset.scale = log.Parent.UVScale;
        uvoffset.value = log.Parent.Radius;

        //First up and down cut

        log = UpAndDown(log, ref uvoffset, Planes.UpPlane, true);
        //log = UpAndDown(log,ref uvoffset, Planes.DownPlane, false);

        //Cut by Right plane

        Vector3 cutPos = log.transform.InverseTransformPoint(CollisionPoint.transform.position);
        Vector3 cutNorm = log.transform.InverseTransformDirection(Planes.RightPlane.transform.up);

        EzySlice.Plane planeRight = new EzySlice.Plane(cutPos, cutNorm);

        SlicedHull result = log.gameObject.Slice(planeRight, ref uvoffset, material);

        //Create Hulls
        GameObject log_lower = result.CreateLowerHull(log.gameObject, material);
        if (!log_lower) return;
        log_lower.SetActive(false);
        GameObject log_upper = result.CreateUpperHull(log.gameObject, material);
        if (!log_upper)
        {
            Destroy(log_lower);
            return;
        }
        log_upper.SetActive(false);


        if (log_lower.GetComponent<MeshFilter>().mesh && log_upper.GetComponent<MeshFilter>().mesh)
        {
            //disable prev fragment
            log.Parent.RemoveFragment(log);
            log.gameObject.SetActive(false);

            //Cut by Left plane
            Vector3 cutNorm2 = log.transform.InverseTransformDirection(Planes.LeftPlane.transform.up);

            EzySlice.Plane planeLeft = new EzySlice.Plane(cutPos, cutNorm2);

            SlicedHull result2 = log_lower.gameObject.Slice(planeLeft, ref uvoffset, material);

            //create Other hull
            GameObject log1_aux = result2.CreateUpperHull(log_lower.gameObject, material);
            Destroy(log_lower);
            if (!log1_aux)
            {
                Destroy(log_upper);
                log.gameObject.SetActive(true);
                log.Parent.AddFragment(log);
                
                return;
            }
            log1_aux.SetActive(false);
           


            //create new fragments
            LogFragment logFragment1 = Instantiate<LogFragment>(log, log.Parent.transform);
            LogFragment logFragment2 = Instantiate<LogFragment>(log, log.Parent.transform);

            logFragment1.CutsForLog.AddCutPlane(planeRight, CutsForLog.UpperHull);
            logFragment2.CutsForLog.AddCutPlane(planeRight, CutsForLog.LowerHull);
            logFragment2.CutsForLog.AddCutPlane(planeLeft, CutsForLog.UpperHull);

            logFragment1.InitAfterInstantiate(log.Parent, log_upper.GetComponent<MeshFilter>().mesh);

            logFragment2.InitAfterInstantiate(log.Parent, log1_aux.GetComponent<MeshFilter>().mesh);

            Destroy(log1_aux);
            Destroy(log_upper);

            bool newlog = false;

            if (logFragment1.Volume < _maxVolume)
            {
                float newMass = logFragment1.Volume * Log.MassCoheficient;
                Log NewLog = Instantiate<Log>(logFragment1.Parent.Prefab, logFragment1.Parent.transform.position, logFragment1.Parent.transform.rotation);
                NewLog.Index = logFragment1.Parent.Index;
                NewLog.transform.localScale = log.Parent.transform.localScale;

                logFragment1.Parent.RecalculateMass();

                NewLog.Prefab = logFragment1.Parent.Prefab;
                logFragment1.transform.parent = NewLog.transform;
                logFragment1.Parent = NewLog;

               

                NewLog.GetComponent<Rigidbody>().mass = newMass;

                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;

                newlog = true;


                //XRGrabInteractable grab = NewLog.AddComponent<XRGrabInteractable>();
                //grab.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Kinematic;

                ApplyImpulse(NewLog, log);

            }

            if (logFragment2.Volume < _maxVolume)
            {
                float newMass = logFragment2.Volume * Log.MassCoheficient;

                logFragment2.Parent.RecalculateMass();
                Log NewLog = Instantiate<Log>(logFragment2.Parent.Prefab, logFragment2.Parent.transform.position, logFragment2.Parent.transform.rotation);
                NewLog.Index = logFragment2.Parent.Index;
                NewLog.transform.localScale = log.Parent.transform.localScale;

                NewLog.Prefab = logFragment2.Parent.Prefab;

                logFragment2.transform.parent = NewLog.transform;
                logFragment2.Parent = NewLog;

               

                NewLog.GetComponent<Rigidbody>().mass = newMass;

                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;
                newlog = true;

                //XRGrabInteractable grab = NewLog.AddComponent<XRGrabInteractable>();
                //grab.movementType = UnityEngine.XR.Interaction.Toolkit.XRBaseInteractable.MovementType.Kinematic;

                ApplyImpulse(NewLog, log);
            }

            logFragment1.Parent.AddFragment(logFragment1);
            logFragment2.Parent.AddFragment(logFragment2);

            logFragment1.gameObject.SetActive(true);
            logFragment2.gameObject.SetActive(true);

            //if(!
            log.Parent.RecalculateMass();
            log.Parent.CheckFragmentCount();

            logFragment1.Parent.CheckMinVolume(_minVolume);
            logFragment2.Parent.CheckMinVolume(_minVolume);

            if (log.Parent.Special)
            {
                logFragment1.Parent.Special = true;
                logFragment2.Parent.Special = true;
            }
            generateNew = newlog;
            if (!newlog)
            {
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Hitting);
                //SoundPlayer.PlaySound(CutSound.Hit, true);
                int xpy = 5;
                //Debug.Log("[AxeCut] NormalCut Simple Hit " + auxiliarCount);
                //auxiliarCount++;
            }
            else
            {
                logFragment1.Parent.Rigidbody.isKinematic = true;
                logFragment2.Parent.Rigidbody.isKinematic = true;
                log.Parent.Rigidbody.isKinematic = true;

                float vol = 0;
                if (logFragment1.Volume <= _maxVolume)
                    vol = logFragment1.Volume;
                if (logFragment2.Volume <= _maxVolume)
                    if (logFragment2.Volume > logFragment1.Volume)
                        vol = logFragment2.Volume;

                if (vol == 0)
                    vol = logFragment2.Volume;

                if (vol < _minVolume)
                {
                    int xpy = 5;
                    //SoundPlayer.PlaySound(CutSound.LittleCut, true);
                    ////Debug.Log("[AxeCut] NormalCut Little " + auxiliarCount);
                    //auxiliarCount++;
                }
                else
                {
                    int xpy = 5;
                    //SoundPlayer.PlaySound(CutSound.MidCut, true);
                    //Debug.Log("[AxeCut] NormalCut Mid " + auxiliarCount);
                    //auxiliarCount++;
                }
            }
            //GenerateMesh(log.Parent);
            Destroy(log.gameObject);
            index++;
            //logFragment1.Parent.GetComponent<LogDispawn>().ResetTimer();
            //logFragment2.Parent.GetComponent<LogDispawn>().ResetTimer();
            //Debug.Log("[AxeCut] NORMAL" + auxiliarCount);
        }
    }

    LogFragment UpAndDown(LogFragment log, ref UVOffset uv, Transform Plane, bool up)
    {
        Material material = log.GetComponent<MeshRenderer>().material;

        Vector3 pos = Plane.position;


        Vector3 cutPos = log.transform.InverseTransformPoint(pos);
        Vector3 cutNorm = log.transform.InverseTransformDirection(Plane.up);

        EzySlice.Plane planeRight = new EzySlice.Plane(cutPos, cutNorm);

        SlicedHull result = log.gameObject.Slice(planeRight, ref uv, material);
        if (result == null)
        {
            return log;
        }
        //Create Hulls
        GameObject log_lower = result.CreateLowerHull(log.gameObject, material);
        if (!log_lower) return log;
        log_lower.SetActive(false);
        GameObject log_upper = result.CreateUpperHull(log.gameObject, material);
        if (!log_upper)
        {
            Destroy(log_lower);
            return log;
        }
        log_upper.SetActive(false);

        if (log_lower.GetComponent<MeshFilter>().mesh && log_upper.GetComponent<MeshFilter>().mesh)
        {
            //disable prev fragment
            log.Parent.RemoveFragment(log);
            log.gameObject.SetActive(false);

            LogFragment log1 = Instantiate<LogFragment>(log, log.Parent.transform);
            LogFragment log2 = Instantiate<LogFragment>(log, log.Parent.transform);

            log1.InitAfterInstantiate(log.Parent, log_upper.GetComponent<MeshFilter>().mesh);
            log2.InitAfterInstantiate(log.Parent, log_lower.GetComponent<MeshFilter>().mesh);

            log1.CutsForLog.AddCutPlane(planeRight, CutsForLog.UpperHull);
            log2.CutsForLog.AddCutPlane(planeRight, CutsForLog.LowerHull);

            Destroy(log_lower);
            Destroy(log_upper);
            Destroy(log.gameObject);
            log1.Parent.AddFragment(log1);
            log2.Parent.AddFragment(log2);

            log1.gameObject.SetActive(true);
            log2.gameObject.SetActive(true);

            
            if (up)
                return log2;
            else return log1;
        }

        return log;
    }


    private void ApplyImpulse(Log NewLog, LogFragment log)
    {
        Vector3 force = NewLog.MassCenter - log.Parent.MassCenter;
        force.Normalize();

        force = log.Parent.transform.TransformDirection(force);
        force += new Vector3(0, 1, 0);

        NewLog.Rigidbody.AddForce(force * ForceMagnitude);
    }

   
    /*private void OnCollisionEnter(Collision collision)
    {
        LogFragment log = collision.collider.GetComponent<LogFragment>();
        if (log)
        {
            if (f && !cutting ) return;
            //checks
            
            f = true;
            cutting = true;
            CollisionPoint.Triggered = false;

        }
    }*/


}
