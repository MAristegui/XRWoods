using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

//public delegate void ObjectGrab(OVRGrabber hand);
public delegate void ObjectRealese();
//  Extends the OVRGrabbable class to add event functionality
public class GrabbableObject : MonoBehaviour //OVRGrabbable
{
    //public event ObjectGrab OnObjectGrab;
    public event ObjectRealese OnObjectRealese;
    [SerializeField] bool PreventOtherAttach = true;


    /*public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        OnObjectGrab?.Invoke(hand);
    }
    
    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity)
    {
        base.GrabEnd(linearVelocity, angularVelocity);
        OnObjectRealese?.Invoke();
    }*/
}
