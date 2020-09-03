using Cinemachine;
using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// An add-on module for CinemachineVirtualCamera that overrides Up.
    /// </summary>
    [SaveDuringPlay, AddComponentMenu("")] // Hide in menu.
    public class CinemachineCustomCameraUp : CinemachineExtension {
        [Tooltip("Use this object's local Up direction.")] public Transform Up = null;

        protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime) {
            if (stage == CinemachineCore.Stage.Body && Up != null) {
                // Override camera up.
                state.ReferenceUp = Up.up;
            }
        }
    }
}
