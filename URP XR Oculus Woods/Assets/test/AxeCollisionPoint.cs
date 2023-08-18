using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeCollisionPoint : MonoBehaviour
{
    public ParticleSystem ShavingParticles;
    public bool Triggered = false;
    public bool TriggeredStump = false;

    [SerializeField] Transform _axeLength;
    [SerializeField] float CheckTime = 0.1f;
    public float AxeLenght { get { return _axeLength.localPosition.magnitude; } }
    [SerializeField] float CollisionRadius = 0.015f;
    [SerializeField] Transform Point1, Point2;
    Vector3 prevPosition;

    [HideInInspector]public Vector3 Velocity;

    public Vector3 Direction
    {
        get { return -transform.right; }
    }

    float timer = 0;

    private void Start()
    {
        prevPosition = transform.position;
        
    }
    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > CheckTime)
        {
            if(timer>0)
                Velocity = (transform.position - prevPosition) / timer;
            prevPosition = transform.position;
            timer = 0;
        }
    }

    public Collider[] Colliders()
    {
        Collider[] clearanceBuffer1; ;
        clearanceBuffer1 = Physics.OverlapCapsule(Point1.position, Point2.position, CollisionRadius);
        //Physics.OverlapSphereNonAlloc(CapsuleCollider.transform.position, CapsuleCollider.radius, clearanceBuffer1);
        // if we don't find anything in the vicinity, reenable collisions!
        return clearanceBuffer1;
    }

    private void OnTriggerEnter(Collider other)
    {
        LogFragment log = other.GetComponent<LogFragment>();
        if (log)
            Triggered = true;

        if(other.tag == "Stump")
            TriggeredStump = true;
    }

    private void OnTriggerExit(Collider other)
    {
        LogFragment log = other.GetComponent<LogFragment>();
        //if (log)
            //Triggered = false;

        if (other.tag == "Stump")
            TriggeredStump = false;
    }

    internal void PlayParticles()
    {
        StartCoroutine(ParticlesAfterTime(0.1f));
    }

    IEnumerator ParticlesAfterTime(float t)
    {
        yield return new WaitForSecondsRealtime(t);

        var particles = Instantiate<ParticleSystem>(ShavingParticles, ShavingParticles.transform.position, ShavingParticles.transform.rotation);

        //Transform Terrain = WoodsTerrain.Instance.transform;
        //particles.collision.SetPlane(0, Terrain);
        
        particles.Play();
    }
}
