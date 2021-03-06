﻿using UnityEngine;

// TODO:
// - Implement GetBestFollowVehicle so it selects the best follow vehicle to frame the lookat vehicle.
// - Instantiate MountedCameraRigs dynamically as needed, instead of taking a scene reference (only necessary if rigs are car-model-specific).

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public class MountedCameraDirector : MonoSingleton<MountedCameraDirector> {
        [SerializeField] private MountedCameraRig rig;

        /// <summary>
        /// Returns the best available follow vehicle to frame the given lookat vehicle.
        /// </summary>
        /// <param name="lookAtVehicle">The object that needs to be in view. Pass null to get a random follow vehicle.</param>
        private static BaseVehicleDriver GetBestFollowVehicle(BaseVehicleDriver lookAtVehicle) {
            BaseVehicleDriver followVehicle = null;
            BaseVehicleDriver[] availableVehicles = WorldObjectManager.Instance.Drivers;
            if (lookAtVehicle == null) {
                followVehicle = ListHelper.GetRandomValue(availableVehicles);
            } else {
                // TODO: Replace with best follow vehicle determination.
                //       Can be done by comparing the priority of the lookAtVehicle generated by a rig on each followVehicle in range.
                followVehicle = ListHelper.GetRandomValue(availableVehicles);
            }
            
            return followVehicle;
        }

        /// <summary>
        /// Returns the best available lookat vehicle for the given rig.
        /// </summary>
        /// <param name="rig">The rig from which the lookat vehicle is viewed.</param>
        private static BaseVehicleDriver GetBestLookAtVehicle(MountedCameraRig rig) {
            BaseVehicleDriver lookAtVehicle = null;
            BaseVehicleDriver[] availableVehicles = WorldObjectManager.Instance.Drivers;
            if (rig.FollowVehicle == null) {
                lookAtVehicle = ListHelper.GetRandomValue(availableVehicles);
            } else {
                lookAtVehicle = rig.GetBestLookAtVehicle(availableVehicles);
            }
            return lookAtVehicle;
        }

        /// <summary>
        /// Returns the best available camera for the given follow and lookat vehicles.
        /// </summary>
        /// <param name="followVehicle">The vehicle the camera will be mounted to. Pass null to determine the best follow vehicle for the given lookat vehicle automatically.</param>
        /// <param name="lookAtVehicle">The vehicle that needs to be in view. Pass null to determine the best lookat vehicle for the given follow vehicle automatically.</param>
        public MountedCamera GetBestMountedCamera(BaseVehicleDriver followVehicle, BaseVehicleDriver lookAtVehicle) {
            rig.FollowVehicle = followVehicle ?? GetBestFollowVehicle(lookAtVehicle);
            return rig.GetBestCamera(lookAtVehicle ?? GetBestLookAtVehicle(rig));
        }
    }
}
