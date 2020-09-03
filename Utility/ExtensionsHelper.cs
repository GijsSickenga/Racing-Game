using UnityEngine;

namespace CinematicCameraSystem {
    /// <summary>
    /// Contains useful extension functions.
    /// </summary>
    public static class ExtensionsHelper {
        /// <summary>
        /// The number of children active in the hierarchy.
        /// </summary>
        public static int GetActiveChildCount(this Transform transform) {
            int activeChildCount = 0;
            foreach (Transform child in transform) {
                if (child.gameObject.activeSelf) {
                    activeChildCount++;
                }
            }
            return activeChildCount;
        }

        /// <summary>
        /// Sets the look rotation of the transform.
        /// </summary>
        public static void SetLookRotation(this Transform transform, Vector3 forward, Vector3 up) {
            Quaternion newRotation = transform.rotation;
            newRotation.SetLookRotation(forward, up);
            transform.rotation = newRotation;
        }
    }
}
