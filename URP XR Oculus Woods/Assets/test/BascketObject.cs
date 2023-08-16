using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class BascketObject : MonoBehaviour
{
    [SerializeField]
    PreventDontDestroyOnLoad PreventDontDestroyOnLoad;
    [SerializeField] public Rigidbody Rigidbody;
    [SerializeField] Throwable Throwable;
    [SerializeField] Log Log;
    [SerializeField] float _threshold = 0.1f;
    float TimeToLock2 = 1;

    [SerializeField] int LockedLayer;
    [SerializeField] int NormalLayer;
    bool _lock;
    public bool _locked;
    float _timer;

    Transform Parent;

    Vector3 pos;
    Quaternion rot;

    float mass;

    Interactable Interactable;

    string Tag;
    string BascketTag = "BascketFloor";

    private void Start()
    {
        Throwable.interactable.onAttachedToHand += Interactable_onAttachedToHand;
        NormalLayer = gameObject.layer;
        Tag = tag;
    }

    private void Interactable_onAttachedToHand(Hand hand)
    {
        Unlock();
    }

    public void InmediateLock(Transform parent, float time, Interactable interactable)
    {
        if (_lock) return;
        _lock = true;
        Parent = parent;

        //Rigidbody.mass = 1;
        //FixedJoint = parent;
        Interactable = interactable;
        TimeToLock2 = time;
        _timer = 0;
        tag = BascketTag;
        if (!_locked)
        {
            PrivateLock();

        }
        transform.localRotation = rot;
        transform.localPosition = pos;
    }

    public void Lock(Transform parent, float time, Interactable interactable)
    {
        if (_lock) return;
        _lock = true;
        Parent = parent;

        //Rigidbody.mass = 1;
        //FixedJoint = parent;
        Interactable = interactable;
        TimeToLock2 = time;
        _timer = 0;
        tag = BascketTag;


    }

    private void PrivateLock()
    {
        _locked = true;
        Log.LockPosition();
        transform.parent = Parent;

        gameObject.layer = LockedLayer;
        
        //Rigidbody.isKinematic = true; 
        //Rigidbody.useGravity = false;
        Rigidbody.constraints =
              RigidbodyConstraints.FreezeRotationX
            | RigidbodyConstraints.FreezeRotationY
            | RigidbodyConstraints.FreezeRotationZ
            | RigidbodyConstraints.FreezePositionX
            | RigidbodyConstraints.FreezePositionY
            | RigidbodyConstraints.FreezePositionZ
            ;
        pos = transform.localPosition;
        rot = transform.localRotation;
    }

    public void Unlock()
    {
        Log.UnlockPosition();
        transform.parent = null;
        _lock = false;

        //Rigidbody.isKinematic = false;
        gameObject.layer = NormalLayer;
        _locked = false;
        Interactable = null;
        tag = Tag;
        PreventDontDestroyOnLoad.Detach();
    }

    public void Update()
    {
        if (_lock)
        {
            if (
                Rigidbody.angularVelocity.magnitude < _threshold
                && Rigidbody.velocity.magnitude < _threshold
                && !Throwable.interactable.attachedToHand
                )
            {
                _timer += Time.deltaTime;
                if (_timer > TimeToLock2)
                {
                    if (!_locked)
                    {
                        PrivateLock();

                    }
                    transform.localRotation = rot;
                    transform.localPosition = pos;
                }
                    
            }
            else 
                _timer = 0;

        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider && collision.collider.tag == BascketTag || collision.rigidbody && collision.rigidbody.tag == BascketTag)
        {
            if(Interactable && Interactable.attachedToHand)
            {
               

                if (Throwable.interactable.attachedToHand)
                    Interactable.attachedToHand.DetachObject(Interactable.gameObject);
                _timer = 1;
                Rigidbody.angularVelocity = Vector3.zero;
                Rigidbody.velocity = Vector3.zero;


                PrivateLock();
            }
        }
    }

}
