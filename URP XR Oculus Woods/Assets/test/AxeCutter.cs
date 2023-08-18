using EzySlice;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AxeCutter : MonoBehaviour
{
    public float MinCutSpeedTest = 0;
    [Header("References")]
    [SerializeField] AxeCutPlanes Planes;
    [SerializeField] AxeCollisionPoint CollisionPoint;
    [SerializeField] AxeCutSounds SoundPlayer;
    [Header("Components")]
    [SerializeField] public Rigidbody Rigidbody;
    //[SerializeField] public Throwable Throwable;
    [Header("Parameters")]
    [SerializeField] float CheckTime = 0.3f;
    [SerializeField] float CheckTime2 = 0.01f;
    [SerializeField] public float AngleThreshold = 0.1f;
    [SerializeField] public float MinCutSpeed = 2;
    [SerializeField] public float MinCutSpeedCritical = 3;

    [SerializeField] float _maxVolume = 0.01f;
    [SerializeField] float _minVolume = 0.001f;
    [SerializeField] float ForceMagnitude = 500;

    public CollisionDetectionMode CollisionMode = CollisionDetectionMode.Continuous;

    [Header("Flags")]
    public bool AttachedToHand = false;

    float timer = 0;
    bool f = false;
    int index = 0;

    bool Stump = false;
    bool cutting = false;

    public static float MaxVolume { get; protected set; }
    public static float MinVolume { get; protected set; }

    [Header("Stamina Consumption")]
    [SerializeField] float Critic = 1;
    [SerializeField] float Normal = 0.4f;
    // Start is called before the first frame update
    void Start()
    {
        MaxVolume = _maxVolume;
        MinVolume = _minVolume;
    }

    private void FixedUpdate()
    {
        
        {
            if (f)
            {
                timer += Time.deltaTime;
                if (timer > CheckTime)
                {
                    timer = 0;
                    f = false;
                }
                if(timer > CheckTime2)
                cutting = false;
            }
        }
    }

    public int ProcessColision(Collider collider, out float velocity)
    {
        velocity = 0;
        if (!collider) return 0;
        LogFragment log = collider.GetComponent<LogFragment>();

        if (log)
        {
            //variables
            Vector3 RealVelocity = CollisionPoint.Velocity;

            bool attached = CheckLogAttachedToHand(log.Parent.gameObject);

            if (attached)
            {
                RealVelocity /= 2;
                /*Hand hand = log.Parent.GetComponent<Throwable>().interactable.attachedToHand;

                if (hand)
                {
                    hand.DetachObject(log.Parent.gameObject);
                    log.Parent.Rigidbody.velocity = RealVelocity;
                }*/

            }
            velocity = RealVelocity.magnitude;
            if (
                //log.Volume < _maxVolume
                //&& 
                //log.Parent.GetFragmentList().Count <= 1
                //|| 
                log.Parent.Stacked || log.Parent.BascketObject._locked
                )
            {
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Hitting);
                SoundPlayer.PlaySound(CutSound.SoftHit, true);
                //Debug.Log("[AxeCut] log.Volume < _maxVolume " + auxiliarCount);
                //auxiliarCount++;
                return 0;
            }

            if (log.Parent.Locked)
            {
                if (RealVelocity.magnitude > MinCutSpeedCritical)
                {
                    //if (!LogStack.OverLogCount())
                    {
                        CriticalCut(log);
                        return 1;
                    }
                }
            }
            else if (RealVelocity.magnitude > MinCutSpeed)
            {
                //if (!LogStack.OverLogCount())
                {
                    NormalCut(log);
                    //WoodsPlayer.Player.UseStamina(Normal * RealVelocity.magnitude);
                    return 2;
                }
            }
            else 
            { 
                
                return 2;
            }
            
        }
        velocity = 0;
        return 0;
       
    }

    bool generateNew = false;

    private void OnCollisionEnter(Collision collision)
    {
        LogFragment log = collision.collider.GetComponent<LogFragment>();
        if (log)
        {
            if (f
            && !cutting
            ) return;
            //checks
            if (!CheckCollisionPoint())
            {
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Handling);
                // SoundPlayer.PlaySound(CutSound.SoftHit, false);
                //Debug.Log("[AxeCut] !CheckCollisionPoint "+auxiliarCount);
                //auxiliarCount++;
                return;
            }
            f = true;
            cutting = true;
            CollisionPoint.Triggered = false;

            Vector3 cutPos = transform.InverseTransformPoint(log.Parent.transform.position);
            Quaternion rot = log.Parent.transform.rotation;

            var colliders = CollisionPoint.Colliders();

            int spend = 0;

            float velocity = 0;

            foreach(var c in colliders)
            {
                int s = ProcessColision(c, out velocity);
                if (s > spend)
                    spend = s;
            }
            /*if (spend == 1)
                WoodsPlayer.Player.UseStamina(Critic);
            else if (spend == 2)
            {
                WoodsPlayer.Player.UseStamina(Normal * velocity);
                if (!generateNew)
                {
                    if (UnityEngine.Random.value <= 0.1f)
                    {
                        log.Parent.GetComponent<BascketObject>().InmediateLock(transform, 0, Throwable.interactable);
                        log.Parent.GetComponent<GrabbedObjectMesh>().ChangeLayer(gameObject.layer);
                        log.Parent.transform.localPosition = cutPos;
                    }
                }
            }*/
           
            generateNew = false;

        }
        else
        {
            if (collision.collider.transform.tag == "Stump")
            {
                Stump = true;
            }

        }

}

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.transform.tag == "Stump")
        {
            Stump = false;
        }
    }

    void NormalCut(LogFragment log)
    {
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
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Handle);
                SoundPlayer.PlaySound(CutSound.SoftHit, false);
                //Debug.Log("[AxeCut] ERROR !log1_aux " + auxiliarCount);
                //auxiliarCount++;
                return;
            }
            log1_aux.SetActive(false);
            CollisionPoint.PlayParticles();


            //create new fragments
            LogFragment log1 = Instantiate<LogFragment>(log, log.Parent.transform);
            LogFragment log2 = Instantiate<LogFragment>(log, log.Parent.transform);

            log1.CutsForLog.AddCutPlane(planeRight, CutsForLog.UpperHull);
            log2.CutsForLog.AddCutPlane(planeRight, CutsForLog.LowerHull);
            log2.CutsForLog.AddCutPlane(planeLeft, CutsForLog.UpperHull);

            log1.InitAfterInstantiate(log.Parent, log_upper.GetComponent<MeshFilter>().mesh);

            log2.InitAfterInstantiate(log.Parent, log1_aux.GetComponent<MeshFilter>().mesh);

            Destroy(log1_aux);
            Destroy(log_upper);

            bool newlog = false;

            if (log1.Volume < _maxVolume)
            {
                float newMass = log1.Volume * Log.MassCoheficient;
                Log NewLog = Instantiate<Log>(log1.Parent.Prefab, log1.Parent.transform.position, log1.Parent.transform.rotation);
                NewLog.Index = log1.Parent.Index;
                NewLog.transform.localScale = log.Parent.transform.localScale;

                log1.Parent.RecalculateMass();

                NewLog.Prefab = log1.Parent.Prefab;
                log1.transform.parent = NewLog.transform;
                log1.Parent = NewLog;

                //Vector3 center = log1.CenterFragment(log.Parent.MassCenter);

                NewLog.GetComponent<Rigidbody>().mass = newMass;
                
                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;

                //center = log.Parent.transform.TransformPoint(center);
                //NewLog.transform.position = 2*NewLog.transform.position - center;
                newlog = true;

                ApplyImpulse(NewLog, log);

            }

            if (log2.Volume < _maxVolume)
            {
                float newMass = log2.Volume * Log.MassCoheficient;

                log2.Parent.RecalculateMass();
                Log NewLog = Instantiate<Log>(log2.Parent.Prefab, log2.Parent.transform.position, log2.Parent.transform.rotation);
                NewLog.Index = log2.Parent.Index;
                NewLog.transform.localScale = log.Parent.transform.localScale;

                NewLog.Prefab = log2.Parent.Prefab;
                
                log2.transform.parent = NewLog.transform;
                log2.Parent = NewLog;

                //Vector3 center = log2.CenterFragment(log.Parent.MassCenter);

                NewLog.GetComponent<Rigidbody>().mass = newMass;

                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;

                //center = log.Parent.transform.TransformPoint(center);

                //NewLog.transform.position = 2*NewLog.transform.position - center;
                newlog = true;
                
                ApplyImpulse(NewLog, log);
            }

            log1.Parent.AddFragment(log1);
            log2.Parent.AddFragment(log2);

            log1.gameObject.SetActive(true);
            log2.gameObject.SetActive(true);

            //if(!
            log.Parent.RecalculateMass();
            log.Parent.CheckFragmentCount();

            log1.Parent.CheckMinVolume(_minVolume);
            log2.Parent.CheckMinVolume(_minVolume);

            if (log.Parent.Special)
            {
                log1.Parent.Special = true;
                log2.Parent.Special = true;
            }
            generateNew = newlog;
            if (!newlog)
            {
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Hitting);
                SoundPlayer.PlaySound(CutSound.Hit, true);
                //Debug.Log("[AxeCut] NormalCut Simple Hit " + auxiliarCount);
                //auxiliarCount++;
            }
            else
            {
                log1.Parent.Rigidbody.isKinematic = true;
                log2.Parent.Rigidbody.isKinematic = true;
                log.Parent.Rigidbody.isKinematic = true;

                float vol = 0;
                if (log1.Volume <= _maxVolume)
                    vol = log1.Volume;
                if (log2.Volume <= _maxVolume)
                    if (log2.Volume > log1.Volume)
                        vol = log2.Volume;

                if (vol == 0)
                    vol = log2.Volume;

                if(vol < _minVolume)
                {
                    SoundPlayer.PlaySound(CutSound.LittleCut, true);
                    ////Debug.Log("[AxeCut] NormalCut Little " + auxiliarCount);
                    //auxiliarCount++;
                }
                else
                {
                    SoundPlayer.PlaySound(CutSound.MidCut, true);
                    //Debug.Log("[AxeCut] NormalCut Mid " + auxiliarCount);
                    //auxiliarCount++;
                }
            }
            //GenerateMesh(log.Parent);
            Destroy(log.gameObject);
            index++;
            log1.Parent.GetComponent<LogDispawn>().ResetTimer();
            log2.Parent.GetComponent<LogDispawn>().ResetTimer();
            //Debug.Log("[AxeCut] NORMAL" + auxiliarCount);
        }
    }

    private void ApplyImpulse(Log NewLog, LogFragment log)
    {
        Vector3 force = NewLog.MassCenter - log.Parent.MassCenter;
        force.Normalize();

        force = log.Parent.transform.TransformDirection(force);
        force += new Vector3(0, 1, 0);

        NewLog.Rigidbody.AddForce(force * ForceMagnitude);
    }

    void CriticalCut(LogFragment log)
    {
        Material material = log.GetComponent<MeshRenderer>().material;
        UVOffset uvoffset = new UVOffset();
        uvoffset.offset = log.Parent.UVOffset;
        uvoffset.scale = log.Parent.UVScale;
        uvoffset.value = log.Parent.Radius;
        //Cut by Right plane

        Vector3 cutPos = log.transform.InverseTransformPoint(CollisionPoint.transform.position);
        Vector3 cutNorm = log.transform.InverseTransformDirection(transform.forward);

        EzySlice.Plane plane = new EzySlice.Plane(cutPos, cutNorm);

        SlicedHull result = log.gameObject.Slice(plane, ref uvoffset, material);

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
        CollisionPoint.PlayParticles();

        log.Parent.RemoveFragment(log);
        log.gameObject.SetActive(false);

        //create fragments
        LogFragment log1 = Instantiate<LogFragment>(log, log.Parent.transform);
        LogFragment log2 = Instantiate<LogFragment>(log, log.Parent.transform);

        log1.CutsForLog.AddCutPlane(plane, CutsForLog.UpperHull);
        log2.CutsForLog.AddCutPlane(plane, CutsForLog.LowerHull);

        log1.InitAfterInstantiate(log.Parent, log_upper.GetComponent<MeshFilter>().mesh);

        log2.InitAfterInstantiate(log.Parent, log_lower.GetComponent<MeshFilter>().mesh);

        //Destroy hulls
        Destroy(log_lower);
        Destroy(log_upper);

        if (log2.Volume < _maxVolume)
        {
            float newMass2 = log2.Volume * Log.MassCoheficient;

            Log NewLog2 = Instantiate<Log>(log2.Parent.Prefab, log2.Parent.transform.position, log2.Parent.transform.rotation);
            NewLog2.Index = log2.Parent.Index;
            NewLog2.transform.localScale = log.Parent.transform.localScale;

            log2.Parent.RecalculateMass();
            NewLog2.Prefab = log2.Parent.Prefab;
            log2.transform.parent = NewLog2.transform;
            log2.Parent = NewLog2;

            log2.transform.localPosition = Vector3.zero;
            //Vector3 center2 = log2.CenterFragment(log.Parent.MassCenter);

            //NewLog2.MeshFilter.mesh = log2.MeshFilter.mesh;
            //NewLog2.GetComponent<MeshCollider>().sharedMesh = log2.MeshFilter.mesh;

            NewLog2.GetComponent<Rigidbody>().mass = newMass2;

            NewLog2.Radius = log.Parent.Radius;

            NewLog2.name = log.Parent.name + " - FL" + index;
            //center2 = log.Parent.transform.TransformPoint(center2);
            //NewLog2.transform.position = 2*NewLog2.transform.position - center2;

            ApplyImpulse(NewLog2, log);
        }

        //creating new log

        Log NewLog = Instantiate<Log>(log1.Parent.Prefab, log1.Parent.transform.position, log1.Parent.transform.rotation);
        NewLog.Index = log1.Parent.Index;
        NewLog.transform.localScale = log.Parent.transform.localScale;

        var PrevParent = log1.Parent;
        float newMass = log1.Volume * Log.MassCoheficient;
        NewLog.Prefab = log1.Parent.Prefab;
        log1.transform.parent = NewLog.transform;
        log1.Parent = NewLog;
        log1.transform.localPosition = Vector3.zero;

        //Vector3 center = log1.CenterFragment(log.Parent.MassCenter);

        //NewLog.MeshFilter.mesh = log1.MeshFilter.mesh;
        //NewLog.GetComponent<MeshCollider>().sharedMesh = log1.MeshFilter.mesh;

        NewLog.GetComponent<Rigidbody>().mass = newMass;
        NewLog.name = log.Parent.name + " - CL" + index;

        NewLog.Radius = log.Parent.Radius;
        //center = log.Parent.transform.TransformPoint(center);
        //NewLog.transform.position = 2*NewLog.transform.position - center;

        ApplyImpulse(NewLog, log);

        //checks
        log1.Parent.AddFragment(log1);
        log2.Parent.AddFragment(log2);

        log1.gameObject.SetActive(true);
        log2.gameObject.SetActive(true);


        //if (!
        log.Parent.CheckFragmentCount();
           // )
           // GenerateMeshCollider(log.Parent);

        log1.Parent.CheckMinVolume(_minVolume);
        log2.Parent.CheckMinVolume(_minVolume);

        if (log.Parent.Special)
        {
            log1.Parent.Special = true;
            log2.Parent.Special = true;
        }

        if (log1.Volume < _minVolume && (log2.Volume <= log1.Volume))
        {
            SoundPlayer.PlaySound(CutSound.LittleCut, true);
        }
        else if (log1.Volume > _maxVolume)
        {
            SoundPlayer.PlaySound(CutSound.BigCut, true); 
        }
        else 
        {
            SoundPlayer.PlaySound(CutSound.MidCut, true);
        }
        
        Destroy(log.gameObject);
        PrevParent.RecalculateMass();
        index++;

        log1.Parent.GetComponent<LogDispawn>().ResetTimer();
        log2.Parent.GetComponent<LogDispawn>().ResetTimer();
        //Debug.Log("[AxeCut] CRITICAL" + auxiliarCount);

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

            //if(!
            //log.Parent.CheckFragmentCount();
            //)
            //GenerateMeshCollider(log.Parent);

            //log1.Parent.CheckMinVolume(_minVolume);
            //log2.Parent.CheckMinVolume(_minVolume);
            if (up)
                return log2;
            else return log1;
        }

        return log;
    }

    public static void GenerateMesh(Log log)
    {
        List<LogFragment> list = log.GetFragmentList();

        CombineInstance[] combine = new CombineInstance[list.Count];

        int i = 0;
        while (i < list.Count)
        {
            combine[i].mesh = list[i].MeshFilter.sharedMesh;
            combine[i].transform = Matrix4x4.identity;

            i++;
        }
        Mesh mesh = new Mesh();
        mesh.CombineMeshes(combine);

        var array = list.ToArray();

        //Initialize
        LogFragment log1 = Instantiate<LogFragment>(list[0], log.transform);
        log1.Mark = true;
        log1.InitAfterInstantiate(log, mesh);

        foreach (var l in array)
        {
            l.Parent.RemoveFragment(l);
            Destroy(l);
        }

        log1.Parent.AddFragment(log1);

        log1.gameObject.SetActive(true);

        log1.Parent.CheckFragmentCount();
        

    }

    bool CheckCollisionPoint()
    {
        bool ret = CollisionPoint.Triggered;
        CollisionPoint.Triggered = false;

        /*Hand hand = Throwable.interactable.attachedToHand;
        if (!hand) return false;*/

        /*Vector3 velocity = hand.GetTrackedObjectVelocity();

        velocity.Normalize();

        float dot = Vector3.Dot(velocity, CollisionPoint.Direction);*/


        Vector3 RealVelocity = CollisionPoint.Velocity;

        float multiplier = 1 + Mathf.Clamp((LogStack.LogCount - 50)/100f,0, 1)*2;
        /*float staminamultiplier = 1 + Mathf.Clamp(
            (1-(WoodsPlayer.Player.Statistics.Stamina) / 100),
            0, 1);*/
        /*ret = ret && dot > AngleThreshold 
            && RealVelocity.magnitude > MinCutSpeedCritical * staminamultiplier * multiplier;
        MinCutSpeedTest = MinCutSpeedCritical * staminamultiplier * multiplier;*/
        return ret;
    }

    bool CheckLogAttachedToHand(GameObject obj)
    {
        return false;// obj.GetComponent<Throwable>().interactable.attachedToHand;
    }

    public void OnGrabAxe()
    {
        Rigidbody.isKinematic = false;
        AttachedToHand = true;

        Rigidbody.collisionDetectionMode = CollisionMode;

        //SoundPlayer.PlayRandomSound(SoundPlayerTag.Handle);
        if (Stump)
            SoundPlayer.PlaySound(CutSound.GrabAxeStump, true);
        else
            SoundPlayer.PlaySound(CutSound.GrabAxe, true);

    }

    public void OnDetachFromHand()
    {
        AttachedToHand = false;
    }
}
