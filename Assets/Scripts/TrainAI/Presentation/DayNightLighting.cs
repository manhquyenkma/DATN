using TrainAI.Core;
using TrainAI.Core.Messages;
using TrainAI.Services;
using UnityEngine;

namespace TrainAI.Presentation
{
    [RequireComponent(typeof(Light))]
    public class DayNightLighting : MonoBehaviour
    {
        public ServiceLocatorSO services;
        public Gradient lightColor;
        public AnimationCurve lightIntensity;
        
        Light _sunLight;

        void Awake()
        {
            _sunLight = GetComponent<Light>();
            if (_sunLight.type != LightType.Directional)
            {
                Debug.LogWarning("[DayNightLighting] Should be attached to a Directional Light.");
            }
            BroadcastService.Subscribe<TimeTickMsg>(OnTimeTick);
        }

        void OnDestroy()
        {
            BroadcastService.Unsubscribe<TimeTickMsg>(OnTimeTick);
        }

        void Start()
        {
            // Initial update
            if (services != null && services.Clock != null)
            {
                UpdateLighting(services.Clock.Hour * 60 + services.Clock.Minute);
            }
        }

        void OnTimeTick(TimeTickMsg msg)
        {
            UpdateLighting(msg.hour * 60 + msg.minute);
        }

        void UpdateLighting(int currentMinutes)
        {
            if (_sunLight == null) return;

            // currentMinutes is from 0 to 1440.
            // 0 = midnight, 360 = 6 AM, 720 = 12 PM, 1080 = 6 PM, 1440 = midnight.
            float t = currentMinutes / 1440f;

            // X rotation: 
            // 6 AM (0.25) -> 0 degrees (horizon)
            // 12 PM (0.5) -> 90 degrees (zenith)
            // 6 PM (0.75) -> 180 degrees (horizon)
            // Midnight (0 / 1.0) -> -90 (nadir)
            float xRot = Mathf.Lerp(-90f, 270f, t);
            transform.rotation = Quaternion.Euler(xRot, 50f, 0f);

            // Color and intensity
            if (lightColor != null)
                _sunLight.color = lightColor.Evaluate(t);
            if (lightIntensity != null)
                _sunLight.intensity = lightIntensity.Evaluate(t);
        }
    }
}
