using System.Collections;
using Cinemachine;
using UnityEngine;

namespace CinematicCameraSystem {
    [RequireComponent(typeof(CinemachineVirtualCamera)), DisallowMultipleComponent]
    public partial class CinematicCamera : MonoBehaviour {
        public virtual bool OverrideLookAt { get { return false; } }

        [SerializeField] private bool overrideWorldUp = false;
        public virtual bool OverrideWorldUp {
            get { return overrideWorldUp; }
        }
        public Vector3 CameraUp { get { return CustomCameraUp?.Up?.up ?? CinematicDirector.Instance.Brain.DefaultWorldUp; } }
        private CinemachineCustomCameraUp CustomCameraUp { get; set; }

        [SerializeField, HideInInspector] private CinemachineVirtualCamera virtualCamera;
        public CinemachineVirtualCamera VirtualCamera {
            get {
                virtualCamera = virtualCamera ?? GetComponent<CinemachineVirtualCamera>();
                return virtualCamera;
            }
        }
        [SerializeField, HideInInspector] private CinemachineComposer composer;
        private CinemachineComposer Composer {
            get {
                composer = composer ?? VirtualCamera?.GetCinemachineComponent<CinemachineComposer>();
                return composer;
            }
        }
        [SerializeField, HideInInspector] private CinemachineTransposer transposer;
        private CinemachineTransposer Transposer {
            get {
                transposer = transposer ?? VirtualCamera?.GetCinemachineComponent<CinemachineTransposer>();
                return transposer;
            }
        }

        private void Start() {
            Initialize();
        }

        private void Initialize() {
            if (!ValidatePrerequisites()) {
                Destroy(gameObject);
                return;
            }

            if (overrideWorldUp) {
                GameObject worldUpObject = new GameObject(gameObject.name + " World Up");
                worldUpObject.transform.rotation = transform.rotation;
                worldUpObject.transform.parent = CinematicDirector.Instance.CameraWorldUps;

                CustomCameraUp = gameObject.AddComponent<CinemachineCustomCameraUp>();
                CustomCameraUp.Up = worldUpObject.transform;
                VirtualCamera.AddExtension(CustomCameraUp);
            }
            
            DisableCamera();
        }

        private bool ValidatePrerequisites() {
            bool ready = true;
            if (!CinematicDirector.Exists) {
                Debug.LogError("CinematicDirector not present in scene. Destroying " + gameObject.name + ".");
                ready = false;
            } else if (VirtualCamera == null) {
                Debug.LogError("VirtualCamera is not set. Destroying " + gameObject.name + ".");
                ready = false;
            }

            return ready;
        }

        public void MakeActiveCamera(Transform target, CinematicCamera previousCamera) {
            if (!OverrideLookAt) { VirtualCamera.LookAt = target; }
            if (previousCamera != this) { StartCoroutine(ActivateCameraPrewarmed(target, previousCamera)); }
        }

        private void DisableCamera() {
            VirtualCamera.Priority = 0;
            VirtualCamera.enabled = false;
        }

        private IEnumerator ActivateCameraPrewarmed(Transform target, CinematicCamera previousCamera) {
            if (Composer != null) {
                // Lock camera rotation to target before prewarming LookAt damping.
                transform.LookAt(VirtualCamera.LookAt, CameraUp);
            }

            if (Transposer != null) {
                // Lock position to target before prewarming Follow damping.
                transform.position = VirtualCamera.Follow.position;
            }

            // Enable incoming camera, then wait a few frames before switching to it to let it prewarm.
            // This hides the camera's readjustment to the target.
            VirtualCamera.enabled = true;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            // Switch cameras.
            previousCamera?.DisableCamera();
            VirtualCamera.Priority = 100;
            yield break;
        }

#if UNITY_EDITOR
        public const string ALIGN_ROTATION_FUNCTION_NAME = "Align Camera Rotation With Track";
        public const string ALIGN_UP_FUNCTION_NAME = "Align Camera Up With Track";

        public void AlignCameraRotationWithTrack() {
            UnityEditor.Undo.RecordObject(transform, ALIGN_ROTATION_FUNCTION_NAME);

            TrackPath track = FindObjectOfType<TrackPath>();
            if (track == null) {
                Debug.LogError("No track path found in scene; cannot align camera with track.", this);
                return;
            }

            if (track.LinearPath == null) {
                track.GenLinearPath();
            }
            
            Vector3 pointOnPath;
            Vector3 trackDirection;
            Vector3 trackRight;
            track.LinearPath.GetClosestPointOnPath(transform.position, out pointOnPath, out trackDirection, out trackRight, true);
            Vector3 trackUp = Vector3.Cross(trackDirection, trackRight);

            Debug.DrawLine(transform.position, pointOnPath, Color.green, 5f);
            Debug.DrawLine(pointOnPath, pointOnPath + trackUp * 2.5f, Color.green, 5f);

            transform.SetLookRotation(trackDirection, trackUp);
        }

        public void AlignCameraUpWithTrack() {
            Vector3 oldForward = transform.forward;
            AlignCameraRotationWithTrack();

            UnityEditor.Undo.RecordObject(transform, ALIGN_UP_FUNCTION_NAME);
            transform.SetLookRotation(Vector3.ProjectOnPlane(oldForward, transform.up).normalized, transform.up);
        }

        private void Reset() {
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
        }
#endif
    }

#if UNITY_EDITOR
    public partial class CinematicCamera {
        public const string OVERRIDE_WORLD_UP_VARIABLE_NAME = nameof(overrideWorldUp);
    }
#endif
}
