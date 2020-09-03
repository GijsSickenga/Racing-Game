using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public class CinematicCameraTriggerManager : MonoSingleton<CinematicCameraTriggerManager> {
        [SerializeField] private TrackPath trackPath;
        public TrackPath TrackPath {
            get {
                if (trackPath == null) {
                    FindTrackPath();
                }
                return trackPath;
            }
        }

        [SerializeField] private List<CinematicCameraTrigger> cameraTriggers = new List<CinematicCameraTrigger>();
        public List<CinematicCameraTrigger> CameraTriggers {
            get {
                UpdateCameraTriggersIfNecessary(false);
                return cameraTriggers;
            }
        }

        public void UpdateCameraTriggersIfNecessary(bool checkForDisabledTriggers) {
            int triggerCount = checkForDisabledTriggers ? transform.GetActiveChildCount() : transform.childCount;
            if (cameraTriggers.Count != triggerCount || cameraTriggers.Contains(null)) {
                UpdateCameraTriggers();
            }
        }

        private void UpdateCameraTriggers() {
            cameraTriggers = new List<CinematicCameraTrigger>(GetComponentsInChildren<CinematicCameraTrigger>());
            cameraTriggers = cameraTriggers.OrderByDescending(x => x.DistanceAlongTrack).ToList();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [ContextMenu("Find Track Path")]
        private void FindTrackPath() {
            trackPath = FindObjectOfType<TrackPath>();
        }

#if UNITY_EDITOR
        private void OnValidate() {
            UpdateCameraTriggers();
        }

        private void Reset() {
            gameObject.name = "Cinematic Triggers";
            FindTrackPath();
        }
#endif
    }
}
