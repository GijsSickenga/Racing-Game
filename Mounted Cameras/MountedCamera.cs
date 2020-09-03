using UnityEngine;

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public class MountedCamera : CinematicCamera {
        public override bool OverrideLookAt { get { return true; } }
        public override bool OverrideWorldUp { get { return false; } }

        public void SetMountedCameraBase(Transform target) {
            // NOTE: Both Follow & LookAt are set to the same target.
            //       The VirtualCamera's LookAt offset points the camera ahead or behind the object it's attached to.
            VirtualCamera.Follow = target;
            VirtualCamera.LookAt = target;
        }
    }
}
