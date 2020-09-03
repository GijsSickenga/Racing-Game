using System.Collections.Generic;
using UnityEngine;
using WeightedRandomization;

// TODO: Hold a reference to the trigger manager in the level instead of using singleton.

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public partial class CinematicCameraTrigger : MonoBehaviour {
        // Linked cameras.
        [SerializeField] private bool isStuntTrigger = false;
        public bool IsStuntTrigger { get { return isStuntTrigger; } }

        [SerializeField] private StuntContainer linkedStunt;
        public StuntContainer LinkedStunt { get { return linkedStunt; } }

        private StuntCameraDirector linkedStuntCameraDirector;
        private StuntCameraDirector LinkedStuntCameraDirector {
            get {
                if (linkedStuntCameraDirector == null) {
                    linkedStuntCameraDirector = linkedStunt?.SpawnedStunt?.GetComponentInChildren<StuntCameraDirector>();
                }
                return linkedStuntCameraDirector;
            }
        }

        [SerializeField] private List<WeightedCinematicCamera> linkedCameras = new List<WeightedCinematicCamera>();
        public List<WeightedCinematicCamera> LinkedCameras { get { return linkedCameras; } }
        public List<WeightedCinematicCamera> LevelCameras { get { return isStuntTrigger ? LinkedStuntCameraDirector?.StuntCameras : linkedCameras; } }

        // Shot determination.
        [SerializeField] private float levelShotWeight = 1;
        [SerializeField] private float mountedShotWeight = 0;

        // Generated from shot weight floats.
        [SerializeField, HideInInspector] private WeightedCinematicShotType[] shotWeights;

        // Track values (read only).
        [SerializeField] private float distanceAlongTrack;
        public float DistanceAlongTrack { get { return distanceAlongTrack; } }
        [SerializeField] private float percentageAlongTrack;
        public float PercentageAlongTrack { get { return percentageAlongTrack; } }
        [SerializeField] private Vector3 closestPointOnTrack;
        public Vector3 ClosestPointOnTrack { get { return closestPointOnTrack; } }
        [SerializeField] private Vector3 trackDirection;
        public Vector3 TrackDirection { get { return trackDirection; } }
        [SerializeField] private Vector3 trackPerpendicular;
        public Vector3 TrackPerpendicular { get { return trackPerpendicular; } }

        public CinematicCamera GetWeightedCamera() {
            CinematicShotType randomShotType = WeightedRandom.Get(shotWeights);

            switch (randomShotType) {
                default:
                case CinematicShotType.Level: {
                    // Get a mounted camera if there are no level cameras available.
                    return GetWeightedLevelCamera() ?? GetBestMountedCamera(WorldObjectManager.Instance.PlayerDriver, null);
                }
                case CinematicShotType.Mounted: {
                    return GetBestMountedCamera(WorldObjectManager.Instance.PlayerDriver, null);
                }
            }
        }

        public CinematicCamera GetWeightedLevelCamera() {
            return WeightedRandom.Get(LevelCameras?.ToArray());
        }

        public CinematicCamera GetBestMountedCamera(BaseVehicleDriver followVehicle, BaseVehicleDriver lookAtVehicle) {
            return MountedCameraDirector.Instance.GetBestMountedCamera(followVehicle, lookAtVehicle);
        }

#if UNITY_EDITOR
        public void UpdateName() {
            string prefix = isStuntTrigger ? "(S) " : "";
            gameObject.name = prefix + "Trigger (" + percentageAlongTrack.ToString("##0.0") + "%)";
        }
        
        public void RecalculateTrackPosition() {
            TrackPath track = CinematicCameraTriggerManager.Instance.TrackPath;
            if (track.LinearPath == null) {
                CinematicCameraTriggerManager.Instance.TrackPath.GenLinearPath();
            }

            float distanceToPoint; // unused
            track.LinearPath.GetClosestPointOnPath(transform.position, out closestPointOnTrack, out trackDirection, out trackPerpendicular, out distanceAlongTrack, out distanceToPoint, false);
            percentageAlongTrack = distanceAlongTrack / track.Distance * 100f;
            transform.position = closestPointOnTrack;

            UpdateName();
            CinematicCameraTriggerManager.Instance.UpdateCameraTriggersIfNecessary(true);
        }

        private void OnValidate() {
            levelShotWeight = Mathf.Max(0, levelShotWeight);
            mountedShotWeight = Mathf.Max(0, mountedShotWeight);

            shotWeights = new WeightedCinematicShotType[] {
                new WeightedCinematicShotType(CinematicShotType.Level, levelShotWeight),
                new WeightedCinematicShotType(CinematicShotType.Mounted, mountedShotWeight)
            };

            UpdateName();
            CinematicCameraTriggerManager.Instance.UpdateCameraTriggersIfNecessary(true);
        }

        private void Reset() {
            CinematicCameraTriggerManager cinematicCameraTriggerManager = FindObjectOfType<CinematicCameraTriggerManager>();
            if (cinematicCameraTriggerManager == null) {
                Debug.LogError("A CinematicCameraTriggerManager is necessary for camera triggers to be parented to. Please place one in the scene prior to placing camera triggers.", gameObject);
                Destroy(this);
                return;
            }

            UpdateName();
            transform.parent = cinematicCameraTriggerManager.transform;
            cinematicCameraTriggerManager.UpdateCameraTriggersIfNecessary(true);
        }
#endif
    }
    
#if UNITY_EDITOR
    public partial class CinematicCameraTrigger {
        // Variable names - used by CinematicCameraTriggerEditor.cs.
        public const string IS_STUNT_TRIGGER_VARIABLE_NAME = nameof(isStuntTrigger);
        public const string LINKED_STUNT_VARIABLE_NAME = nameof(linkedStunt);
        public const string LINKED_CAMERAS_VARIABLE_NAME = nameof(linkedCameras);
        public const string LEVEL_SHOT_WEIGHT_VARIABLE_NAME = nameof(levelShotWeight);
        public const string MOUNTED_SHOT_WEIGHT_VARIABLE_NAME = nameof(mountedShotWeight);
        public const string CLOSEST_POINT_ON_TRACK_VARIABLE_NAME = nameof(closestPointOnTrack);
        public const string DISTANCE_ALONG_TRACK_VARIABLE_NAME = nameof(distanceAlongTrack);
        public const string PERCENTAGE_ALONG_TRACK_VARIABLE_NAME = nameof(percentageAlongTrack);
        public const string TRACK_DIRECTION_VARIABLE_NAME = nameof(trackDirection);
        public const string TRACK_PERPENDICULAR_VARIABLE_NAME = nameof(trackPerpendicular);
    }
#endif
}
