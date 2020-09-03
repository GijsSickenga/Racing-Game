using System.Linq;
using UnityEngine;

namespace CinematicCameraSystem {
    [RequireComponent(typeof(SphereCollider))]
    public class ShakeableCameraDetector : MonoBehaviour {
        public ShakeableCameraUnityEvent OnCollideWithShakeableCamera = new ShakeableCameraUnityEvent();

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.layer == CinematicDirector.Instance.Settings.CinematicCameraLayer) {
                ShakeableCamera camera = other.GetComponent<ShakeableCamera>();
                if (camera != null) {
                    OnCollideWithShakeableCamera.Invoke(camera);
                }
            }
        }

        private void Reset() {
            SphereCollider triggerCollider = GetComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            
            // Scale collider to largest renderer extents.
            triggerCollider.radius = 0.25f;
            
            gameObject.layer = CinematicDirector.Instance.Settings.CameraShakerLayer;
        }
    }
}
