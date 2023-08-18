using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Log : MonoBehaviour
{
    public bool Special = false;
    public XRGrabInteractable Interactable;
    //protected static Material _internalMaterial;
    [SerializeField] float _dispawnTime = 3;
    float _startBurntTime2 = 0.5f;
    public Material InternalMaterial;
    public Rigidbody Rigidbody;
    public MeshFilter MeshFilter;

    LogStack stack;

    public bool Stacked = false;

    float burntValue = 0;

    
    public MeshCollider MeshCollider1;

    public Log Prefab;

    [SerializeField] LogFragment FirsFragment;


    private List<LogFragment> FragmentList;

    public static float MassCoheficient;

    public Vector3 MassCenter;

    public Vector2 UVOffset;
    public Vector2 UVScale;
    public float Radius;

    [SerializeField] protected Graph Graph;

    bool flag = false;

    public bool _destroy = false;
    public bool _destroyAfterSeconds = false;

    DispawnLog DispawnLog;

    public bool _inFire = false;

    public BascketObject BascketObject;

    public int Index;

    private void Awake()
    {
        Interactable = GetComponent<XRGrabInteractable>();
        Rigidbody = GetComponent<Rigidbody>();
        DispawnLog = GetComponent<DispawnLog>();
        BascketObject = GetComponent<BascketObject>();
        MeshFilter = GetComponent<MeshFilter>();
        MeshCollider1 = GetComponent<MeshCollider>();

        FirsFragment = GetComponentInChildren<LogFragment>();
        FragmentList = new List<LogFragment>();

        LogStack.AddLog();
        NormalCollisionDetection = Rigidbody.collisionDetectionMode;
        if (FirsFragment)
        {
            FirsFragment.Init(this);
            Radius = FirsFragment.Radius;

            FragmentList.Add(FirsFragment);

            MassCoheficient = Rigidbody.mass/FirsFragment.Volume;

            MassCenter = FirsFragment.MassCenter;

            Graph.AddNode(FirsFragment.Node);

        }
    }

    public void AddFragment(LogFragment fragment)
    {
        fragment.transform.localPosition = Vector3.zero;
        fragment.transform.localScale = Vector3.one;
        FragmentList.Add(fragment);
        Graph.AddNode(fragment.Node);

    }

    public void SetSpecial()
    {
        Special = true;
        if (FragmentList != null)
        {
            foreach (var f in FragmentList)
            {
                f.MeshRenderer.material = InternalMaterial;
            }
        }
        else
        {
            var list = GetComponentsInChildren<LogFragment>();
            foreach (var f in list)
            {
                f.MeshRenderer.material = InternalMaterial;
            }
        }
    }

    bool first = true;
    float timer = 0;
    private void FixedUpdate()
    {
        flag = false;
        if (!Locked)
        {
            Rigidbody.constraints = RigidbodyConstraints.None;
            Rigidbody.isKinematic = false;
            Rigidbody.collisionDetectionMode = NormalCollisionDetection;
        }

        if (_destroy)
        {
            timer += Time.fixedDeltaTime;
            if (timer > _dispawnTime*(1+ _startBurntTime2))
            {

                if (DispawnLog)
                    DispawnLog.Dispawn();
                else
                    Destroy(this.gameObject);
            }

            burntValue = timer / _dispawnTime;
            if(burntValue > _startBurntTime2)
                SetBurntValue();
            PunishPlayer();
            
        }
        else
        {
            timer -= Time.fixedDeltaTime;
            if (timer < 0) timer = 0;
            else
            {
                burntValue = timer / _dispawnTime;
                if (burntValue > _startBurntTime2)
                    SetBurntValue();
                PunishPlayer();
            }
        }

        
    }

    void PunishPlayer()
    {
        //if (Interactable.attachedToHand)
        if (Interactable.isSelected)
        {
            if (!detached)
                StartCoroutine(DetachAfterSeconds(0.3f));
            //Falta woodsPlayer
            //if (timer > 0.1f * _dispawnTime)
            //    WoodsPlayer.Player.Statistics.ForceTemperature(70, 3);
        }
    }


    internal void NotSpecial(Material material)
    {
        Special = false;
        if (FragmentList != null)
        {
            foreach (var f in FragmentList)
            {
                f.MeshRenderer.material = material;
            }
        }
        else
        {
            var list = GetComponentsInChildren<LogFragment>();
            foreach (var f in list)
            {
                f.MeshRenderer.material = material;
            }
        }

    }

    bool detached = false;
    IEnumerator DetachAfterSeconds(float time)
    {
        if (!detached)
        {
            detached = true;
            yield return new WaitForSecondsRealtime(time);
            //if(Interactable.attachedToHand)
            //    Interactable.attachedToHand.DetachObject(this.gameObject);
            detached = false;
        }

        yield return null;
    }

    void SetBurntValue()
    {
        foreach(var l in FragmentList)
        {
            float value = Mathf.Clamp(burntValue - _startBurntTime2, 0, 1);
            l.MeshRenderer.material.SetFloat("_AlphaMask",  value* 10);
        }
    }
    CollisionDetectionMode NormalCollisionDetection;
    public bool OnShed = false;

    internal void SetStack(LogStack logStack)
    {
        stack = logStack;
        
        
        Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        Rigidbody.isKinematic = true;
        Locked = true;
        Stacked = true;
    }

    internal void BurnInFire()
    {
        foreach(var f in FragmentList)
        {
            f.MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        _destroy = true;
        DispawnLog.Setup();
        timer = 0;
    }

    public void RemoveFragment(LogFragment fragment)
    {
        if (FragmentList.Contains(fragment))
        {
            FragmentList.Remove(fragment);
            Graph.RemoveNode(fragment.Node);
        }
    }

    public float Volume()
    {
        float ret = 0;

        foreach(var f in FragmentList)
        {
            if (f)
                ret += f.Volume;
        }

        return ret;
    }

    public void RecalculateMass()
    {
        float vol = Volume();
        Rigidbody.mass = vol * MassCoheficient/2;
    }

    public bool CheckFragmentCount()
    {
        if (FragmentList.Count == 0)
        {
            Destroy(this.gameObject);
            return true;
            //Debug.LogError("Mass Destroyed: " + Rigidbody.mass);
        }
        flag = true;
        StartCoroutine(CheckGraph());
        return false;
        //else
           // Debug.LogError("Mass: " + Rigidbody.mass);
    }

    private IEnumerator CheckGraph()
    {
        while (flag)
            yield return null;
        yield return new WaitForSecondsRealtime(0.01f);
        Graph.CheckUnconnectedGraph(this);
    }

    public List<LogFragment> GetFragmentList()
    {
        return FragmentList;
    }

    public bool Locked { get; protected set; }

    public void LockPosition()
    {
        Rigidbody.constraints = 
              RigidbodyConstraints.FreezeRotationX 
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezePositionX
            //| RigidbodyConstraints.FreezePositionY
            | RigidbodyConstraints.FreezePositionZ
            ;
        Rigidbody.isKinematic = false;
        Rigidbody.collisionDetectionMode = NormalCollisionDetection;
        Locked = true;
    }

    public void UnlockPosition()
    {
        Rigidbody.constraints = RigidbodyConstraints.None;

        Rigidbody.isKinematic = false;
        Rigidbody.collisionDetectionMode = NormalCollisionDetection;
        StartCoroutine(UnlockAfterSeconds(.3f));

        if (stack)
        {
            stack.Respawn();
            stack.RemoveLog(this);
            Stacked = false;
            stack = null;
        }

    }

    IEnumerator UnlockAfterSeconds(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        Locked = false;
    }

    IEnumerator CheckAfterSeconds()
    {
        yield return new WaitForSecondsRealtime(10);
        float vol = Volume();
        /*if (Volume() < AxeCutter.MinVolume)
        {
            Destroy();
        }*/
    }
    IEnumerator DestroyAfterSeconds(float t)
    {
        yield return new WaitForSecondsRealtime(t);
        
    }

    public void CheckMinVolume(float minVolume)
    {
        float vol = Volume();
        if (Volume() < minVolume)
        {
            StartCoroutine(DestroyAfterSeconds(5));
        }
        StartCoroutine(CheckAfterSeconds());
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        LogStack.RemoveLog();
    }

    public void SaveFromFire()
    {
        foreach (var f in FragmentList)
        {
            f.MeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }
        _destroy = false;
    }
}
