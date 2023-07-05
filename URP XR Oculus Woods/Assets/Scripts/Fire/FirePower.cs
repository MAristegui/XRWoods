using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Woods.Game.Ambient
{
    public class FirePower : MonoBehaviour
    {
        public static float FireTemperature;
        public static FirePower Instance { get; protected set; }
        [SerializeField] AudioSource AudioSource;
        [SerializeField] GameObject FireHeat;

        [Header("Fire Levels Prefabs")]
        [SerializeField] FireLevelData[] FireLevels;

        [Header("Current Level")]
        [SerializeField] FireLevel CurrentFireLevel;
        public float Power = 50;
        [SerializeField] float _maxPower = 100;
        [SerializeField] float _minPower = 5;
        [SerializeField] float Fuel;

        [Header("Parameters")]
        [SerializeField] float HKa = 50;
        [SerializeField] float FireConsumption = 0.1f;
        [SerializeField] float FireConsumptionMultiplier = 10;
        [SerializeField] float Temp;
        [SerializeField] float Dist;
        [SerializeField] float D = 0.5f;

        [SerializeField] float FuelConsumption = 0.5f;

        public float LightIntensity = 1;

        private void Awake()
        {
            Instance = this;
        }

        /**
         * This method updates the fire's power level based on the amount of fuel consumed, 
         * and calculates the appropriate FireLevel based on the current PowerValue. 
         * If the PowerValue exceeds the maximum or falls below the minimum, it is clamped accordingly. 
         * The method also handles the audio for the fire, and adjusts the fuel level as it is consumed.
         */

        private void Update()
        {
            if (Power > _maxPower)
                Power = _maxPower;
            if (Power < _minPower)
                Power = _minPower;

            if(Power >= FireLevels[0].PowerValue && Power <= FireLevels[1].PowerValue)
            {
                CalculateLevel(0, 1);
            }
            if (Power > FireLevels[1].PowerValue && Power <= FireLevels[2].PowerValue)
            {
                CalculateLevel(1, 2);
            }
            if (Power > FireLevels[2].PowerValue && Power <= FireLevels[3].PowerValue)
            {
                CalculateLevel(2, 3); 
            }

            // Uncomment this after finish the implementation of day/night circle
            /**if (!WoodsDayCycle.Instance.enabled)
            {
                //FireHeat.SetActive(false);
                return;
            }*/

        Power -= Time.deltaTime * FireConsumption;

            // Uncomment this after finish the implementation of woodsPlayer
            //FireTemperature = Temp = Temperature(WoodsPlayer.Player.Camera.transform.position);

            if (Power <= _minPower)
            {
                AudioSource.Stop();
            }
            else if (!AudioSource.isPlaying) 
            {
                AudioSource.Play();
            }
            //FireHeat.SetActive(true);

            if (Fuel > 0)
            {
                Power += FuelConsumption * Time.deltaTime;
                Fuel -= FuelConsumption * Time.deltaTime;
            }
            if (Fuel < 0)
                Fuel = 0;
        }


        public void AddFuel(float fuel)
        {
            Fuel += fuel;
        }

        /**
         * This method calculates the fire consumption, smoke and flame particle effects, fire pit, heat, lighting, 
         * and scale based on the power level of the fire. It uses the "Lerp" function to interpolate between values 
         * from two different "FireLevels" structs, which are defined elsewhere in the code. The result is applied to 
         * the "CurrentFireLevel" object.
         */
        void CalculateLevel(int index0, int index1)
        {
            float t = (Power - FireLevels[index0].PowerValue) / (FireLevels[index1].PowerValue - FireLevels[index0].PowerValue);
            FireConsumption = Mathf.Lerp(
                FireLevels[index0].FireConsumption,
                FireLevels[index1].FireConsumption,
                t) * FireConsumptionMultiplier;

            //SMOKE
            var SmokeMain = CurrentFireLevel.Smoke.main;
            SmokeMain.startLifetime = Lerp(
                FireLevels[index0].Smoke.main.startLifetime,
                FireLevels[index1].Smoke.main.startLifetime,
                t);
            SmokeMain.startSpeed = Lerp(
                FireLevels[index0].Smoke.main.startSpeed,
                FireLevels[index1].Smoke.main.startSpeed,
                t);
            SmokeMain.simulationSpeed = Mathf.Lerp(
                FireLevels[index0].Smoke.main.simulationSpeed,
                FireLevels[index1].Smoke.main.simulationSpeed,
                t);
            SmokeMain.maxParticles = (int)Mathf.Lerp(
                FireLevels[index0].Smoke.main.maxParticles,
                FireLevels[index1].Smoke.main.maxParticles,
                t);

            var SmokeShape = CurrentFireLevel.Smoke.shape;
            SmokeShape.angle = Mathf.Lerp(
                FireLevels[index0].Smoke.shape.angle,
                FireLevels[index1].Smoke.shape.angle,
                t);
            SmokeShape.radius = Mathf.Lerp(
                FireLevels[index0].Smoke.shape.radius,
                FireLevels[index1].Smoke.shape.radius,
                t);

            var SmokeEmission = CurrentFireLevel.Smoke.emission;
            SmokeEmission.rateOverTime = Lerp(
               FireLevels[index0].Smoke.emission.rateOverTime,
               FireLevels[index1].Smoke.emission.rateOverTime,
               t);

            var SmokeTransform = CurrentFireLevel.Smoke.transform;

            SmokeTransform.localPosition = Vector3.Lerp(
               FireLevels[index0].Smoke.transform.localPosition,
               FireLevels[index1].Smoke.transform.localPosition,
               t);

            //Flame
            var FlameMain = CurrentFireLevel.Flame.main;
            FlameMain.startLifetime = Lerp(
                FireLevels[index0].Flame.main.startLifetime,
                FireLevels[index1].Flame.main.startLifetime,
                t);
            FlameMain.startSpeed = Lerp(
                FireLevels[index0].Flame.main.startSpeed,
                FireLevels[index1].Flame.main.startSpeed,
                t);
            FlameMain.startSize = Lerp(
                 FireLevels[index0].Flame.main.startSize,
                 FireLevels[index1].Flame.main.startSize,
                 t);
            /*
            FlameMain.simulationSpeed = Mathf.Lerp(
                FireLevels[index0].Flame.main.simulationSpeed,
                FireLevels[index1].Flame.main.simulationSpeed,
                t);
                /**/
        var FlameShape = CurrentFireLevel.Flame.shape;
            FlameShape.radius = Mathf.Lerp(
                FireLevels[index0].Flame.shape.radius,
                FireLevels[index1].Flame.shape.radius,
                t);

            var FlameEmission = CurrentFireLevel.Flame.emission;
            FlameEmission.rateOverTime = Lerp(
                 FireLevels[index0].Flame.emission.rateOverTime,
                 FireLevels[index1].Flame.emission.rateOverTime,
                 t);


            //Bottom Flames
            var BottonMain = CurrentFireLevel.BottomFlames.main;
            BottonMain.startLifetime = Lerp(
                 FireLevels[index0].BottomFlames.main.startLifetime,
                 FireLevels[index1].BottomFlames.main.startLifetime,
                 t);

            //Fire Pit

            var PitMain = CurrentFireLevel.FirePit.main;
            PitMain.startLifetime = Lerp(
                 FireLevels[index0].FirePit.main.startLifetime,
                 FireLevels[index1].FirePit.main.startLifetime,
                 t);


            var PitMain2 = CurrentFireLevel.FirePit2.main;
            PitMain2.startLifetime = Lerp(
                 FireLevels[index0].FirePit2.main.startLifetime,
                 FireLevels[index1].FirePit2.main.startLifetime,
                 t);

            //Heat - Pending for remove
            /*
            var HeatTransform = CurrentFireLevel.Heat.transform;
            HeatTransform.localPosition = Vector3.Lerp(
               FireLevels[index0].Heat.transform.localPosition,
               FireLevels[index1].Heat.transform.localPosition,
               t);
            /**/
            //Lighting

            var FireLight = CurrentFireLevel.FireLight;
            FireLight.PositionChangeSpeed = Mathf.Lerp(
                FireLevels[index0].FireLight.PositionChangeSpeed,
                FireLevels[index1].FireLight.PositionChangeSpeed,
                t); 
            FireLight.MovementPower = Mathf.Lerp(
                 FireLevels[index0].FireLight.MovementPower,
                 FireLevels[index1].FireLight.MovementPower,
                 t);
            FireLight.IntensityChangePower = Mathf.Lerp(
                FireLevels[index0].FireLight.IntensityChangePower,
                FireLevels[index1].FireLight.IntensityChangePower,
                t) * LightIntensity;
            FireLight.IntensityChangeSpeed = Mathf.Lerp(
                FireLevels[index0].FireLight.IntensityChangeSpeed,
                FireLevels[index1].FireLight.IntensityChangeSpeed,
                t);
            FireLight.BaseIntensity = Mathf.Lerp(
                    FireLevels[index0].FireLight.BaseIntensity,
                    FireLevels[index1].FireLight.BaseIntensity,
                    t) * LightIntensity;

            //scale
            transform.localScale = Vector3.Lerp(
                FireLevels[index0].transform.localScale, 
                FireLevels[index1].transform.localScale, 
                t);
        }

        /*
         * The method calculates the interpolation for the constant, constantMin, constantMax, 
         * and curveMultiplier properties of the ParticleSystem.MinMaxCurve object.
         * It returns a new ParticleSystem.MinMaxCurve object that is a linear interpolation between c0 and c1 based on t.
         * */
        ParticleSystem.MinMaxCurve Lerp(ParticleSystem.MinMaxCurve c0, ParticleSystem.MinMaxCurve c1, float t)
        {
            ParticleSystem.MinMaxCurve ret = c0;

            ret.constant = Mathf.Lerp(c0.constant, c1.constant, t);
            ret.constantMin = Mathf.Lerp(c0.constantMin, c1.constantMin, t);
            ret.constantMax = Mathf.Lerp(c0.constantMax, c1.constantMax, t);
            ret.curveMultiplier = Mathf.Lerp(c0.curveMultiplier, c1.curveMultiplier, t);

            return ret;
        }

        /**
         * This method calculates the temperature at a given position in space based on the distance from the object's
         * transform position and its power. The temperature is calculated using an inverse distance formula and a clamp
         * function to ensure it stays within a range of 0 to 100. The result is returned as a float value.
         */
        public float Temperature(Vector3 Position)
        {
            float Ta = Power;

            float distance = Vector3.Distance(Position, transform.position);
            Dist = distance;
            //if (distance > D)   Whi is this commented?
            {
                float Tb = Ta / ((distance + D) * HKa);
                Tb = Mathf.Clamp(Tb, 0, 100);

                return Tb;
            }
        }
    }
}