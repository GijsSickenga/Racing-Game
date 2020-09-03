using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Contains values determining the nature of a ShakeableCamera's shake.
    /// </summary>
    [CreateAssetMenu(fileName = "Shake Settings", menuName = "RacingGame/Shakeable Camera Settings")]
    public class ShakeableCameraSettings : ScriptableObject {
        [Header("Shake Generation Settings")]
        [SerializeField, Tooltip("Higher values mean an object needs to go faster and pass by closer in order to shake the camera. Default is 1.")]
        private float magnitudeThreshold = 1f;
        public float MagnitudeThreshold { get { return magnitudeThreshold; } }

        [SerializeField, Tooltip("Higher values mean the camera shakes more violently. Default is 1.")]
        private float amplitudeScalar = 1f;
        public float AmplitudeScalar { get { return amplitudeScalar; } }

        [SerializeField, Tooltip("What portion of the shake magnitude constitutes the duration of the shake. 0.5 means half the magnitude in seconds. Default is 0.125.")]
        private float durationScalar = 0.125f;
        public float DurationScalar { get { return durationScalar; } }

        [SerializeField, Tooltip("The maximum shake time in seconds. Default is 2.2.")]
        private float maxDuration = 2.2f;
        public float MaxDuration { get { return maxDuration; } }

        [SerializeField, Tooltip("The distance from the camera at which shake will be reduced proportional to distance. Anything closer causes maximum shake. Default is 0.75.")]
        private float falloffDistance = 0.75f;
        public float FalloffDistance { get { return falloffDistance; } }

        [SerializeField, Tooltip("The maximum distance from which this camera can be shaken. Default is 2.5.")]
        private float maxDistance = 2.5f;
        public float MaxDistance { get { return maxDistance; } }

        [Header("Cinemachine Noise Limits")]
        [SerializeField, Tooltip("The maximum heaviness of the camera's shake. Default is 20.")]
        private float maxAmplitude = 20f;
        public float MaxAmplitude { get { return maxAmplitude; } }

        [SerializeField, Tooltip("The maximum oscillation speed of the camera's shake. Default is 0.08.")]
        private float maxFrequency = 0.08f;
        public float MaxFrequency { get { return maxFrequency; } }

        /// <summary>
        /// The ratio of shake frequency to amplitude at max amplitude.
        /// </summary>
        public float ShakeFrequencyRatio {
            get { return maxFrequency / maxAmplitude; }
        }
    }
}
