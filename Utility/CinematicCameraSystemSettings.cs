using Cinemachine;
using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Global settings for the cinematic camera system.
    /// </summary>
    public class CinematicCameraSystemSettings : ScriptableObject {
        [NoiseSettingsProperty] public NoiseSettings ShakeableCameraNoiseSettings;
        [Layer] public int CinematicCameraLayer;
        [Layer] public int CameraShakerLayer;
        [Tooltip("Minimum delay between camera shots in seconds.")] public float MinimumDelayBetweenCameraShots;
    }
}
