using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeightedRandomization;

namespace CinematicCameraSystem {
    [DisallowMultipleComponent]
    public class StuntCameraDirector : MonoBehaviour {
        [SerializeField] private List<WeightedCinematicCamera> stuntCameras = new List<WeightedCinematicCamera>();
        public List<WeightedCinematicCamera> StuntCameras { get { return stuntCameras; } }

        public CinematicCamera GetRandomStuntCamera() {
            return WeightedRandom.Get(stuntCameras.ToArray());
        }

#if UNITY_EDITOR
        private void Reset() {
            gameObject.name = "Stunt Cameras";
            UpdateStuntCameras();
        }

        private void OnValidate() {
            UpdateStuntCameras();
        }

        [ContextMenu("Update Stunt Cameras")]
        private void UpdateStuntCameras() {
            bool dirty = false;
            int initialCount = stuntCameras.Count;

            // Remove duplicates.
            stuntCameras = stuntCameras.GroupBy(x => x.Parameter).Select(g => g.First()).ToList();
            // Remove null entries.
            for (int i = stuntCameras.Count - 1; i > -1; i--) {
                if (stuntCameras[i]?.Parameter == null) {
                    stuntCameras.RemoveAt(i);
                }
            }
            dirty = initialCount != stuntCameras.Count;

            // Add any newly added cameras.
            CinematicCamera[] childCameras = GetComponentsInChildren<CinematicCamera>();
            foreach (CinematicCamera childCamera in childCameras) {
                if (!stuntCameras.Exists(x => x.Parameter == childCamera)) {
                    stuntCameras.Add(new WeightedCinematicCamera(childCamera, 1));
                    dirty = true;
                }
            }

            if (dirty) { UnityEditor.EditorUtility.SetDirty(this); }
        }
#endif
    }
}
