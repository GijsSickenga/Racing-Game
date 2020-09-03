using UnityEditor;
using UnityEngine;

namespace CinematicCameraSystem.EditorScripts {
    [CustomEditor(typeof(CinematicCameraTriggerManager))]
    public class CinematicCameraTriggerManagerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            CinematicCameraTriggerManager script = (CinematicCameraTriggerManager)target;
            script.UpdateCameraTriggersIfNecessary(true);
        }
    }
}
