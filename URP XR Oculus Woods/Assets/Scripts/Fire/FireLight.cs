using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Woods.Game.Ambient
{
    /**
     *  This class controls the behavior of a fire light source. It changes the color, position, and intensity of the light
     *  based on a series of settings defined in the script. The colors are changed randomly using a set of preset colors,
     *  and the position is animated using a simple trigonometric function. The intensity of the light is also changed using
     *  a trigonometric function, with an adjustable power value.
    */
    public class FireLight : MonoBehaviour
    {

        Light Light;

        [SerializeField] Color[] Colors;
        [SerializeField] float ColorChangeSpeed = 1;
        [SerializeField] public float PositionChangeSpeed = 1;
        [SerializeField] public float MovementPower = 100;
        [SerializeField] public float IntensityChangeSpeed = 1;
        [SerializeField] public float IntensityChangePower = 1;

        public float BaseIntensity;

        Color _prevColor;
        Color _nextColor;

        // Start is called before the first frame update
        void Start()
        {
            Light = GetComponent<Light>();
            BaseIntensity = Light.intensity;
        }

        float _colorTimer = 0;
        float _positionTimer = 0;
        float _intensityTimer = 0;

        // Update is called once per frame
        void FixedUpdate()
        {
            _colorTimer += Time.fixedDeltaTime * ColorChangeSpeed;
            _positionTimer += Time.fixedDeltaTime * PositionChangeSpeed;
            _intensityTimer += Time.fixedDeltaTime * IntensityChangeSpeed;

            if (_colorTimer < 1)
            {
               // _nextColor = Colors[_index];
                Color newColor = Color.Lerp(_prevColor, _nextColor, _colorTimer);

                Light.color = newColor;
            }
            else
            {
                _nextColor = Colors[(int)(Random.value * (Colors.Length - 1))];
                _prevColor = Light.color;
                Light.color = _nextColor;
                _colorTimer = 0;
            }

            Vector3 pos = transform.position;
            pos += new Vector3(
                Mathf.Sin(Mathf.Sin(_positionTimer) * 4) * MovementPower, 
                Mathf.Sin(Mathf.Sin(_positionTimer) * 4) * MovementPower, 
                Mathf.Sin(Mathf.Cos(_positionTimer) * 4) * MovementPower);
            transform.position = pos;

            
            Light.intensity = BaseIntensity + Mathf.Sin(Mathf.Cos(_intensityTimer) * 4) * IntensityChangePower;
            
        }
    }
}