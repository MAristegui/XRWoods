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
        Debug.LogError(log);
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
                        //CriticalCut(log);
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
        Debug.LogError(collision.gameObject.name);
        LogFragment log = collision.collider.GetComponent<LogFragment>();
        Debug.LogError(log);
        if (log)
        {
            if (f && !cutting) return;
            //checks
            /*if (!CheckCollisionPoint())
            {
                //SoundPlayer.PlayRandomSound(SoundPlayerTag.Handling);
                // SoundPlayer.PlaySound(CutSound.SoftHit, false);
                //Debug.Log("[AxeCut] !CheckCollisionPoint "+auxiliarCount);
                //auxiliarCount++;
                return;
            }*/
            f = true;
            cutting = true;
            CollisionPoint.Triggered = false;

            Vector3 cutPos = transform.InverseTransformPoint(log.Parent.transform.position);
            Quaternion rot = log.Parent.transform.rotation;

            var colliders = CollisionPoint.Colliders();
            Debug.LogError(colliders.Length);
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
            //CollisionPoint.PlayParticles();


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

                //Vector3 center = log1.CenterFragment(log.Parent.MassCenter);

                NewLog.GetComponent<Rigidbody>().mass = newMass;
                
                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;

                //center = log.Parent.transform.TransformPoint(center);
                //NewLog.transform.position = 2*NewLog.transform.position - center;
                newlog = true;

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

                //Vector3 center = log2.CenterFragment(log.Parent.MassCenter);

                NewLog.GetComponent<Rigidbody>().mass = newMass;

                NewLog.Radius = log.Parent.Radius;
                NewLog.name = log.Parent.name + " - FL" + index;

                //center = log.Parent.transform.TransformPoint(center);

                //NewLog.transform.position = 2*NewLog.transform.position - center;
                newlog = true;
                
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
                SoundPlayer.PlaySound(CutSound.Hit, true);
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
            logFragment1.Parent.GetComponent<LogDispawn>().ResetTimer();
            logFragment2.Parent.GetComponent<LogDispawn>().ResetTimer();
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
