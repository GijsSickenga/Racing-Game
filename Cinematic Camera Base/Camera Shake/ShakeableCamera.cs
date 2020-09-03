using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using UnityEngine;

// TODO (Gijs): Guarantee camera has the correct noise component to prevent nullrefs.
namespace CinematicCameraSystem {
    [RequireComponent(typeof(CinemachineVirtualCamera)), DisallowMultipleComponent]
    public class ShakeableCamera : MonoBehaviour {
        [System.Serializable]
        private class Shake {
            public Shake(float magnitude, float duration, float timeToNearestPoint) {
                // A shake tweens itself towards zero over its duration.
                // Ease up to max magnitude, then ease back down to zero for smooth effect.
                DOTween.To(() => this.Magnitude, x => this.Magnitude = x, magnitude, timeToNearestPoint).SetEase(Ease.InQuart).OnComplete(() =>
                    DOTween.To(() => this.Magnitude, x => this.Magnitude = x, 0, duration).SetEase(Ease.OutCirc)
                );
            }
            
            // Default is slightly above zero to prevent instant removal in Update().
            public float Magnitude = float.Epsilon;
        }

        [Header("Settings")]
        [SerializeField] private ShakeableCameraSettings settings;

        [Header("References")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineBasicMultiChannelPerlin cameraNoise;

        // Auto added.
        [SerializeField, HideInInspector] private SphereCollider triggerCollider;

        private List<Shake> shakes = new List<Shake>();

        private void Start() {
            if (!CinematicDirector.Exists) {
                Debug.LogError("CinematicDirector not present in scene. Disabling shake on " + gameObject.name + ".");
                Destroy(this);
                return;
            } else if (settings == null) {
                Debug.LogError("Settings not set for ShakeableCamera component on " + gameObject.name + ". Disabling shake.");
                Destroy(this);
                return;
            }

            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = settings.MaxDistance;
            triggerCollider.isTrigger = true;
        }
        
        public void ShakeCamera(float magnitude, float distanceToCameraAtNearestPoint, float timeToNearestPoint) {
            float distanceScalar = Mathf.Clamp01(settings.FalloffDistance / distanceToCameraAtNearestPoint);
            magnitude *= distanceScalar * settings.AmplitudeScalar;

            // Filter out forces too low to shake the camera.
            if (magnitude < settings.MagnitudeThreshold) { return; }

            float duration = Mathf.Clamp(magnitude * settings.DurationScalar, 0, settings.MaxDuration);
            magnitude = Mathf.Clamp(magnitude, 0, settings.MaxAmplitude);
            shakes.Add(new Shake(magnitude, duration, timeToNearestPoint));
        }

        private void Update() {
            if (shakes.Count == 0) { return; }

            float shakeAmplitude = 0;
            for (int i = shakes.Count - 1; i >= 0; i--) {
                if (shakes[i].Magnitude <= 0) {
                    shakes.RemoveAt(i);
                } else {
                    shakeAmplitude += shakes[i].Magnitude;
                }
            }

            if (shakes.Count == 0) {
                cameraNoise.m_AmplitudeGain = 0;
                cameraNoise.m_FrequencyGain = 0;
                return;
            }

            cameraNoise.m_AmplitudeGain = Mathf.Clamp(shakeAmplitude, 0, settings.MaxAmplitude);
            cameraNoise.m_FrequencyGain = Mathf.Clamp(shakeAmplitude * settings.ShakeFrequencyRatio, 0, settings.MaxFrequency);
        }

#if UNITY_EDITOR
        private void Reset() {
            gameObject.layer = CinematicDirector.Instance.Settings.CinematicCameraLayer;

            if (virtualCamera == null) { virtualCamera = GetComponent<CinemachineVirtualCamera>(); }

            cameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (cameraNoise == null) { cameraNoise = virtualCamera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>(); }
            cameraNoise.m_NoiseProfile = CinematicDirector.Instance.Settings.ShakeableCameraNoiseSettings;
            cameraNoise.m_AmplitudeGain = 0;
            cameraNoise.m_FrequencyGain = 0;
        }

        private void OnValidate() {
            if (settings == null || triggerCollider == null) { return; }
            triggerCollider.radius = settings.MaxDistance;
        }

        private void OnDrawGizmosSelected() {
            if (settings == null) { return; }
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, settings.FalloffDistance);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, settings.MaxDistance * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
        }
#endif
    }
}
