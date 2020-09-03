using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Represents an object that can cause a ShakeableCamera to shake.
    /// </summary>
    [RequireComponent(typeof(Rigidbody)), DisallowMultipleComponent]
    public class CameraShaker : MonoBehaviour {
        [SerializeField, Min(0), Tooltip("The heaviness of the shake caused by this object. Default is 1, meaning shake force equals velocity.")]
        private float shakeScalar = 1f;
        [SerializeField] private GameObject shakeableCameraDetector;
        [SerializeField] private Rigidbody body;

        private void Awake() {
            ShakeableCameraDetector cameraDetectorScript = shakeableCameraDetector.GetComponent<ShakeableCameraDetector>();
            if (cameraDetectorScript != null) {
                cameraDetectorScript.OnCollideWithShakeableCamera.AddListener(OnCollideWithShakeableCamera);
            }
        }

        private void OnCollideWithShakeableCamera(ShakeableCamera camera) {
            // Calculate nearest point car will pass by camera.
            Vector3 nearestExpectedPointToCamera = MathHelper.NearestPointOnLine(transform.position, transform.forward, camera.transform.position);
            float distanceToCameraAtNearestPoint = Vector3.Distance(nearestExpectedPointToCamera, camera.transform.position);
            float distanceScaler = Mathf.Clamp01(1f / distanceToCameraAtNearestPoint);
            float shakeForce = body.velocity.magnitude * shakeScalar;

    #if UNITY_EDITOR
            Debug.DrawLine(transform.position, transform.position + transform.forward * 1000f, Color.red, 10f);
            Debug.DrawLine(nearestExpectedPointToCamera, camera.transform.position, Color.blue, 10f);
    #endif

            float distanceToNearestExpectedPointToCamera = Vector3.Distance(transform.position, nearestExpectedPointToCamera);
            float timeToNearestPoint = distanceToNearestExpectedPointToCamera / body.velocity.magnitude;

            camera.ShakeCamera(shakeForce, distanceToCameraAtNearestPoint, timeToNearestPoint);
        }

        private void Reset() {
            if (body == null) { body = GetComponent<Rigidbody>(); }

            if (shakeableCameraDetector == null) {
                ShakeableCameraDetector detectorScript = GetComponentInChildren<ShakeableCameraDetector>();
                if (detectorScript == null) {
                    shakeableCameraDetector = new GameObject("ShakeableCameraDetector");
                    shakeableCameraDetector.transform.parent = transform;
                    shakeableCameraDetector.transform.localPosition = Vector3.zero;
                    shakeableCameraDetector.AddComponent<ShakeableCameraDetector>();
                } else {
                    shakeableCameraDetector = detectorScript.gameObject;
                }
            }
        }
    }
}
