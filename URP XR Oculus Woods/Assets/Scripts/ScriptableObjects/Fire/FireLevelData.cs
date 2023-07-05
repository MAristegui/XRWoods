using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Woods.Game.Ambient;


/**
    This class represents a fire structure in the game. It is a Scriptable Object that can be used by other 
    scripts to generate and control fire in different parts of the game. The class contains several nested 
    subclasses that represent different aspects of the fire, such as smoke emission, flame appearance,
    and light intensity.
*/
[CreateAssetMenu]
public class FireLevelData : ScriptableObject
{

    public float PowerValue;
    public float FireConsumption;
    public TransformData transform;

    public SmokeData Smoke;
    public FlameData Flame;
    public FireLightData FireLight;
    public BottomFireData BottomFlames;
    public FirePitData FirePit;
    public FirePitData FirePit2;

}

/**
    A struct that represents the smoke emission of the fire.
*/
[System.Serializable]
public struct SmokeData
{
    public ParticlesMain main;
    public ParticlesShape shape;
    public ParticlesEmission emission;
    public Vector3 localPosition;
    public TransformData transform;
}
/**
    A struct that represents the flame appearance of the fire.
*/
[System.Serializable]
public struct FlameData
{
    public ParticlesMain main;
    public ParticlesShape shape;
    public ParticlesEmission emission;
}

/**
    A struct that represents the light intensity emitted by the fire.
*/
[System.Serializable]
public struct FireLightData
{
    public float PositionChangeSpeed;
    public float MovementPower;
    public float IntensityChangePower;
    public float IntensityChangeSpeed;
    public float BaseIntensity;
}

/**
   A struct that represents the fire at the bottom of the fireplace.
*/
[System.Serializable]
public struct BottomFireData
{
    public ParticlesMain main;
}

/**
    A struct that represents the fire in a fire pit.
*/
[System.Serializable]
public struct FirePitData
{
    public ParticlesMain main;
}

/**
    A struct that contains the parameters used in the particle system simulation.
*/
[System.Serializable]
public struct ParticlesMain
{
    public ParticleSystem.MinMaxCurve startLifetime;
    public ParticleSystem.MinMaxCurve startSpeed;
    public float simulationSpeed;
    public float maxParticles;
    public ParticleSystem.MinMaxCurve startSize;
}

/**
    A struct that contains the parameters used in the particle system shape.
*/
[System.Serializable]
public struct ParticlesShape
{
    public float angle;
    public float radius;
}

/**
    A struct that contains the parameters used in the particle system emission rate.
*/
[System.Serializable]
public struct ParticlesEmission
{
    public float rateOverTime;
}

/**
    A struct that contains the parameters for an specific transform.
*/
[System.Serializable]
public struct TransformData
{
    public Vector3 localPosition;
    public Vector3 localScale;
}