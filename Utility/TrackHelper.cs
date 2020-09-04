using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Contains helper functions for calculations involving the track.
    /// </summary>
    public static class TrackHelper {
        /// <summary>
        /// Returns the relative offset of the target from the origin on the track.
        /// X = relative horizontal offset, Y = relative distance along track.
        /// </summary>
        public static Vector2 GetRelativePosition2D(BaseVehicleDriver targetVehicle, BaseVehicleDriver originVehicle) {
            if (targetVehicle?.Controller == null || originVehicle?.Controller == null) { return new Vector2(Mathf.Infinity, Mathf.Infinity); }
            Vector3 originPosition = new Vector3(originVehicle.Controller.ClosestPathDistanceToPoint, 0f, originVehicle.Controller.ClosestPathDistanceTraveled);
            Vector3 targetPosition = new Vector3(targetVehicle.Controller.ClosestPathDistanceToPoint, 0f, targetVehicle.Controller.ClosestPathDistanceTraveled);
            float totalTrackDistance = 1f / targetVehicle.PathProgress * targetPosition.z;

            // Get shortest relative distance along track in case shortest path crosses the finish line.
            Vector3 relativeDistance = targetPosition - originPosition;
            float inverseRelativeZ = (totalTrackDistance - Mathf.Abs(relativeDistance.z)) * -Mathf.Sign(relativeDistance.z);
            float shortestRelativeZ = Mathf.Abs(relativeDistance.z) < Mathf.Abs(inverseRelativeZ) ? relativeDistance.z : inverseRelativeZ;

            return new Vector2(relativeDistance.x, shortestRelativeZ);
        }

        /// <summary>
        /// Returns the relative offset of the target from the origin on the track.
        /// X = relative horizontal offset, Z = relative distance along track.
        /// </summary>
        public static Vector3 GetRelativePosition3D(BaseVehicleDriver targetVehicle, BaseVehicleDriver originVehicle) {
            Vector2 relativeDriverPosition = GetRelativePosition2D(targetVehicle, originVehicle);
            return new Vector3(relativeDriverPosition.x, 0, relativeDriverPosition.y);
        }

        /// <summary>
        /// Pass in the result of GetRelativePosition2D to get the dot product value of the target's position relative to its origin.
        /// </summary>
        public static float GetRelativeAngleDot(Vector2 relativePositionOnTrack) {
            return Vector2.Dot(relativePositionOnTrack.normalized, Vector2.up);
        }

        /// <summary>
        /// Returns the quadrant of the target vehicle on the track relative to the origin vehicle.
        /// </summary>
        public static RelativeQuadrant GetRelativeQuadrant(BaseVehicleDriver targetDriver, BaseVehicleDriver originDriver) {
            Vector2 relativePosition = GetRelativePosition2D(targetDriver, originDriver);
            RelativeDirection relativeX = relativePosition.x < 0f ? RelativeDirection.Left : RelativeDirection.Right;
            RelativeDirection relativeZ = relativePosition.y < 0f ? RelativeDirection.Back : RelativeDirection.Front;
            RelativeQuadrant quadrant = (RelativeQuadrant)(relativeX | relativeZ);
            return quadrant;
        }
    }
}
