using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Manages the cinematic camera view during a race.
    /// This is a scene-based Singleton, and it should be present in every track scene.
    /// </summary>
    [DisallowMultipleComponent]
    public class CinematicDirector : MonoSingleton<CinematicDirector> {
        [Header("Manually Set From Prefab")]
        [SerializeField] private CinematicCameraSystemSettings settings;
        public CinematicCameraSystemSettings Settings { get { return settings; } }
        [SerializeField] private CinemachineBrain brain;
        public CinemachineBrain Brain { get { return brain; } }
        [SerializeField] private Transform cameraWorldUps;
        public Transform CameraWorldUps { get { return cameraWorldUps; } }

        [Header("Automatically Set From Scene")]
        [SerializeField] private BaseVehicleDriver target;
        public BaseVehicleDriver Target {
            get { return target; }
            set { target = value; }
        }

        private bool activeOnMainDisplay = false;
        private float shotChangeWaitTimer = Mathf.Infinity;

        private CinematicCameraTrigger currentTrigger;
        public CinematicCameraTrigger CurrentTrigger {
            get { return currentTrigger; }
            private set {
                if (currentTrigger == value || shotChangeWaitTimer < Settings.MinimumDelayBetweenCameraShots) { return; }
                currentTrigger = value;
                ActiveCamera = currentTrigger.GetWeightedCamera();
                shotChangeWaitTimer = 0;
            }
        }

        private CinematicCamera activeCamera = null;
        public CinematicCamera ActiveCamera {
            get { return activeCamera; }
            private set {
                if (value == null || !value.gameObject.activeSelf) { return; }
                value.MakeActiveCamera(target.transform, activeCamera);
                activeCamera = value;
            }
        }

        /// <summary>
        /// Switches the player camera off and cinematic camera on, or vice versa.
        /// </summary>
        public void ToggleActiveStateOnMainDisplay(bool active) {
            if (active && target == null) { FindTarget(); }

            activeOnMainDisplay = active;
            if (RacerCamera.Exists) { RacerCamera.Instance.ToggleActiveState(!active); }
            brain?.gameObject.SetActive(active);

            if (activeOnMainDisplay) {
                SwitchToWeightedCameraFromLastTrigger();
            }
        }

        // TODO: Handle initialization of CinematicDirector at scene load.
        protected override void Awake() {
            base.Awake();
            Initialize();
        }

        // TODO: Handle initialization of CinematicDirector at scene load.
        private void Initialize() {
            if (ValidatePrerequisites()) {
                // Disable the cinematic camera by default.
                brain.gameObject.SetActive(false);
            } else {
                Debug.LogWarning("Destroying CinematicDirector.");
                Destroy(gameObject);
            }
        }

        private bool ValidatePrerequisites() {
            bool ready = true;
            if (settings == null) {
                Debug.LogError("CinematicDirector has no Settings set.");
                ready = false;
            }
            if (brain == null) {
                Debug.LogError("CinematicDirector has no Brain set.");
                ready = false;
            }
            if (cameraWorldUps == null) {
                Debug.LogError("CinematicDirector has no WorldUp set.");
                ready = false;
            }
            if (!CinematicCameraTriggerManager.Exists) {
                Debug.LogError("No instance of CinematicCameraTriggerManager present in scene.");
                ready = false;
            }

            return ready;
        }

        private void OnEnable() {
            if (GameManager.Exists) {
                GameManager.Instance.OnRaceFinished += ActivateOnMainDisplay;
            }
            ToggleActiveStateOnMainDisplay(false);
        }

        private void OnDisable() {
            if (GameManager.Exists) {
                GameManager.Instance.OnRaceFinished -= ActivateOnMainDisplay;
            }
        }

        private void Update() {
            if (shotChangeWaitTimer < Settings.MinimumDelayBetweenCameraShots) {
                shotChangeWaitTimer += Time.deltaTime;
            }

            if (!activeOnMainDisplay) { return; }

            SwitchToWeightedCameraFromLastTrigger();
        }

        private void SwitchToWeightedCameraFromLastTrigger() {
            // Make sure we have a target vehicle and the target has a Controller.
            float distanceTravelled = Target?.Controller?.ClosestPathDistanceTraveled ?? -1;
            if (distanceTravelled == -1) { return; }

            List<CinematicCameraTrigger> cameraTriggers = CinematicCameraTriggerManager.Instance.CameraTriggers;
            if (cameraTriggers.Count == 0) { return; }

            // Default trigger is the first one behind the finish, in case target is beyond the finish but before any other triggers.
            CinematicCameraTrigger closestTriggerBehindTarget = cameraTriggers[0];
            // Picks the closest trigger behind the target and switches to one of its cameras.
            foreach (CinematicCameraTrigger cameraTrigger in CinematicCameraTriggerManager.Instance.CameraTriggers) {
                if (cameraTrigger.DistanceAlongTrack < distanceTravelled) {
                    closestTriggerBehindTarget = cameraTrigger;
                    break;
                }
            }
            CurrentTrigger = closestTriggerBehindTarget;
        }

        private void ActivateOnMainDisplay() {
            ToggleActiveStateOnMainDisplay(true);
        }

        [ContextMenu("Find Target")]
        private void FindTarget() {
            target = FindObjectOfType<PlayerVehicleDriver>();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
