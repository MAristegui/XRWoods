using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Woods.Game.Ambient
{
    /**
     * This class represents a fire in the game. It includes various properties that control the fire's behavior, 
     * such as its power value, consumption rate, and particle systems for smoke, flame, bottom flames, and fire pit effects.
     */

    public class FireLevel : MonoBehaviour
    {
        public float PowerValue;
        public float FireConsumption;
        public ParticleSystem Smoke;
        public ParticleSystem Flame;
        //public Transform Heat;
        public ParticleSystem BottomFlames;
        public ParticleSystem FirePit;
        public ParticleSystem FirePit2;
        //public Light Light;
        public FireLight FireLight;

    }
}