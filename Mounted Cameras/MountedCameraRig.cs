using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeightedRandomization;

// TODO (Gijs):
// - Add prediction to GetBestLookAtVehicle & GetBestCamera so it can get a shot of the lookat vehicle driving into the camera frame.

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public class MountedCameraRig : MonoBehaviour {
        [System.Serializable]
        public class WeightedMountedCamera : WeightedParameter<MountedCamera> {
            public WeightedMountedCamera(MountedCamera parameter, float weight) : base(parameter, weight) { }
        }

        [Header("Camera Targeting Options")]
        [SerializeField] private float minRelativeDistance;
        public float MinRelativeDistance { get { return minRelativeDistance; } }
        [SerializeField] private float idealRelativeDistance;
        public float IdealRelativeDistance { get { return idealRelativeDistance; } }
        [SerializeField] private float maxRelativeDistance;
        public float MaxRelativeDistance { get { return maxRelativeDistance; } }

        // Translates to a dot product, so range is [0, 90].
        private const float MAX_CULL_ANGLE = 90f;

        [SerializeField, Range(0, MAX_CULL_ANGLE)] private float cullAngle;
        public float CullAngle { get { return cullAngle; } }
        public float CullAngleDot { get { return Mathf.Cos(Mathf.Deg2Rad * cullAngle); } }
        [SerializeField, Range(0, MAX_CULL_ANGLE)] private float idealAngle;
        public float IdealAngle { get { return idealAngle; } }
        public float IdealAngleDot { get { return Mathf.Cos(Mathf.Deg2Rad * idealAngle); } }

        [Header("Camera Targets")]
        // TODO: Remove this since it is only used in editor for debugging.
        [SerializeField] private BaseVehicleDriver lookAtVehicle;
        public BaseVehicleDriver LookAtVehicle {
            get { return lookAtVehicle; }
            set { lookAtVehicle = value; }
        }

        [SerializeField] private BaseVehicleDriver followVehicle;
        public const string FOLLOW_TARGET_VARIABLE_NAME = nameof(followVehicle); // This string is used by MountedCameraRigEditor.cs.
        public Transform FollowVehicleViewTransform { get { return followVehicle?.View?.transform; } }
        public BaseVehicleDriver FollowVehicle {
            get { return followVehicle; }
            set {
                followVehicle = value;
                UpdateFollowVehicle();
            }
        }

        [Header("Camera Quadrants")]
        // TODO: Add a nicer way than 4 separate lists to define cameras per quadrant and assign weights in inspector. Preferably custom editor.
        [SerializeField] private WeightedMountedCamera[] frontLeftWeightedCameras;
        [SerializeField] private WeightedMountedCamera[] frontRightWeightedCameras;
        [SerializeField] private WeightedMountedCamera[] backLeftWeightedCameras;
        [SerializeField] private WeightedMountedCamera[] backRightWeightedCameras;

        [Header("Auto Generated (Read Only)")]
        [SerializeField] private List<MountedCamera> frontFacingCameras = new List<MountedCamera>();
        [SerializeField] private List<MountedCamera> rearFacingCameras = new List<MountedCamera>();

        // Not shown in inspector. Only marked SerializeField so adjustments in OnValidate perpetuate when scene is saved.
        [SerializeField] private Dictionary<RelativeQuadrant, WeightedMountedCamera[]> camerasPerQuadrant = new Dictionary<RelativeQuadrant, WeightedMountedCamera[]>();

        ////////////////////////////////////
        ////////// INITIALIZATION //////////
        ////////////////////////////////////
        
        private void Awake() {
            Initialize();
        }

        private void Initialize() {
            UpdateCameraDictionary();
        }

        /////////////////////////////////////////////////
        ////////// LOOKAT TARGET DETERMINATION //////////
        /////////////////////////////////////////////////

        public BaseVehicleDriver GetBestLookAtVehicle(BaseVehicleDriver[] targetVehicles) {
            BaseVehicleDriver bestLookAtVehicle = null;
            float highestPriority = -1;
            float targetPriority;

            foreach (BaseVehicleDriver targetVehicle in targetVehicles) {
                targetPriority = -1;

                Vector2 relativePositionOnTrack = TrackHelper.GetRelativePosition2D(targetVehicle, followVehicle);
                float relativeAngleDot = TrackHelper.GetRelativeAngleDot(relativePositionOnTrack);

                if (InCameraArea(relativePositionOnTrack, relativeAngleDot)) {
                    targetPriority = GetTargetingPriority(relativePositionOnTrack.y, relativeAngleDot);
                }
                
                if (targetPriority > highestPriority) {
                    bestLookAtVehicle = targetVehicle;
                    highestPriority = targetPriority;
                }
            }
            
            return bestLookAtVehicle;
        }

        ////////////////////////////////////
        ////////// TARGET CULLING //////////
        ////////////////////////////////////

        private bool InCameraArea(Vector2 relativePositionOnTrack, float relativeAngleDot) {
            return InCameraRange(relativePositionOnTrack.y) && InCameraAngle(relativeAngleDot);
        }

        private bool InCameraRange(float relativeDistanceAlongTrack) {
            float relativeDistanceMagnitude = Mathf.Abs(relativeDistanceAlongTrack);
            return relativeDistanceMagnitude > minRelativeDistance && relativeDistanceMagnitude < maxRelativeDistance;
        }

        private bool InCameraAngle(float relativeAngleDot) {
            return Mathf.Abs(relativeAngleDot) > CullAngleDot;
        }

        ///////////////////////////////////////////
        ////////// TARGET PRIORITIZATION //////////
        ///////////////////////////////////////////

        private float GetTargetingPriority(float relativeDistance, float relativeAngleDot) {
            return DistancePriority(relativeDistance) * DotPriority(relativeAngleDot);
        }

        private float DistancePriority(float relativeDistance) {
            float relativeDistanceMagnitude = Mathf.Abs(relativeDistance);
            bool lerpFromMinDistance = relativeDistanceMagnitude < idealRelativeDistance;
            float startingPoint = lerpFromMinDistance ? minRelativeDistance : maxRelativeDistance;
            float priority = Mathf.InverseLerp(startingPoint, idealRelativeDistance, relativeDistanceMagnitude);
            return Mathf.SmoothStep(0, 1, priority);
        }

        private float DotPriority(float relativeAngleDot) {
            float priority = 1f - Mathf.Abs(IdealAngleDot - Mathf.Abs(relativeAngleDot));
            return Mathf.SmoothStep(0, 1, priority);
        }

        ////////////////////////////////////////////////
        ////////// CAMERA ANGLE DETERMINATION //////////
        ////////////////////////////////////////////////

        public MountedCamera GetRandomFrontFacingCamera() {
            return ListHelper.GetRandomValue(frontFacingCameras);
        }

        public MountedCamera GetRandomRearFacingCamera() {
            return ListHelper.GetRandomValue(rearFacingCameras);
        }

        public MountedCamera GetRandomCamera() {
            int randomIndex = Random.Range(0, 2);
            return randomIndex == 0 ? GetRandomFrontFacingCamera() : GetRandomRearFacingCamera();
        }

        /// <summary>
        /// Returns the best available camera to frame the target vehicle.
        /// </summary>
        public MountedCamera GetBestCamera(BaseVehicleDriver targetVehicle) {
            if (targetVehicle == null) { return GetRandomFrontFacingCamera(); }
#if UNITY_EDITOR
            LookAtVehicle = targetVehicle;
#endif
            RelativeQuadrant quadrant = TrackHelper.GetRelativeQuadrant(targetVehicle, followVehicle);
            return WeightedRandom.Get(GetQuadrantCameras(quadrant));
        }

        private WeightedMountedCamera[] GetQuadrantCameras(RelativeQuadrant quadrant) {
            return camerasPerQuadrant[quadrant];
        }

        /////////////////////////////////////////
        ////////// SETTINGS VALIDATION //////////
        /////////////////////////////////////////

        private void OnValidate() {
            ClampTargetingValues();
            UpdateFollowVehicle();
            UpdateCameraLists();
        }

        private void ClampTargetingValues() {
            minRelativeDistance = Mathf.Max(0, minRelativeDistance);
            maxRelativeDistance = Mathf.Max(minRelativeDistance, maxRelativeDistance);
            idealRelativeDistance = Mathf.Clamp(idealRelativeDistance, minRelativeDistance, maxRelativeDistance);
            idealAngle = Mathf.Clamp(idealAngle, 0, cullAngle);
        }

        private void UpdateFollowVehicle() {
            foreach (MountedCamera camera in frontFacingCameras) {
                camera?.SetMountedCameraBase(FollowVehicleViewTransform);
            }
            foreach (MountedCamera camera in rearFacingCameras) {
                camera?.SetMountedCameraBase(FollowVehicleViewTransform);
            }
        }

        private void UpdateCameraDictionary() {
            camerasPerQuadrant = new Dictionary<RelativeQuadrant, WeightedMountedCamera[]> {
                [RelativeQuadrant.FrontLeft] = frontLeftWeightedCameras,
                [RelativeQuadrant.FrontRight] = frontRightWeightedCameras,
                [RelativeQuadrant.BackLeft] = backLeftWeightedCameras,
                [RelativeQuadrant.BackRight] = backRightWeightedCameras
            };
        }

        private void UpdateCameraLists() {
            // Create a list of unique front-facing cameras.
            if (frontLeftWeightedCameras != null && frontRightWeightedCameras != null) {
                IEnumerable<MountedCamera> frontLeftCameras = frontLeftWeightedCameras.Select(x => (MountedCamera)x.Parameter);
                IEnumerable<MountedCamera> frontRightCameras = frontRightWeightedCameras.Select(x => (MountedCamera)x.Parameter);
                frontFacingCameras = (frontLeftCameras).Union(frontRightCameras).ToList();
            }

            // Create a list of unique rear-facing cameras.
            if (backLeftWeightedCameras != null && backRightWeightedCameras != null) {
                IEnumerable<MountedCamera> backLeftCameras = backLeftWeightedCameras.Select(x => (MountedCamera)x.Parameter);
                IEnumerable<MountedCamera> backRightCameras = backRightWeightedCameras.Select(x => (MountedCamera)x.Parameter);
                rearFacingCameras = (backLeftCameras).Union(backRightCameras).ToList();
            }
        }
    }
}
