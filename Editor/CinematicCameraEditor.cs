using UnityEditor;
using UnityEngine;

namespace CinematicCameraSystem.EditorScripts {
    [CustomEditor(typeof(CinematicCamera)), CanEditMultipleObjects]
    public class CinematicCameraEditor : Editor {
        public static bool ShowGizmos = true;

        private SerializedProperty overrideWorldUpProperty;

        private CinematicCamera Script { get { return (CinematicCamera)target; } }

        private void OnEnable() {
            overrideWorldUpProperty = serializedObject.FindProperty(CinematicCamera.OVERRIDE_WORLD_UP_VARIABLE_NAME);
        }

        public override void OnInspectorGUI() {
            CustomEditorHelper.DrawPropertyWithTooltip(overrideWorldUpProperty, "Overriding the world up allows this camera's vertical axis to be freely oriented in space.");

            EditorGUILayout.HelpBox("NOTE: The green gizmo line indicates the world up for this camera. The blue gizmo line indicates the forward relative to world up.", MessageType.Info);

            if (overrideWorldUpProperty.boolValue) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Snapping Camera Rotation", EditorStyles.boldLabel);
                if (GUILayout.Button(CinematicCamera.ALIGN_ROTATION_FUNCTION_NAME)) {
                    Script.AlignCameraRotationWithTrack();
                }
                
                if (GUILayout.Button(CinematicCamera.ALIGN_UP_FUNCTION_NAME)) {
                    Script.AlignCameraUpWithTrack();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("RacingGame/Cinematic Camera System/Toggle Camera Gizmos #g")]
        private static void ToggleGizmos() {
            ShowGizmos = !ShowGizmos;
        }

        //////////// GIZMOS ////////////
        private const float GIZMO_SCALE = 0.25f;
        private static readonly Vector3 CAMERA_DIMENSIONS = new Vector3(GIZMO_SCALE, GIZMO_SCALE, GIZMO_SCALE * 1.5f);
        private const float LINE_MAGNITUDE = 8 * GIZMO_SCALE;
        private static readonly Color TRANSLUCENT_RED = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawCameraGizmos(CinematicCamera camera, GizmoType gizmoType) {
            if (!ShowGizmos) { return; }
            Matrix4x4 originalMatrix = Gizmos.matrix;
            Gizmos.matrix = camera.transform.localToWorldMatrix;
            float handleSize = HandleUtility.GetHandleSize(camera.transform.position);
            float scaledLineMagnitude = (Gizmos.matrix * Vector3.up).magnitude * LINE_MAGNITUDE * handleSize;
            Vector3 scaledCameraDimensions = CAMERA_DIMENSIONS * handleSize;

            Gizmos.color = TRANSLUCENT_RED;
            Gizmos.DrawCube(Vector3.zero, scaledCameraDimensions);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, scaledCameraDimensions);

            Gizmos.matrix = originalMatrix;

            if (camera.OverrideWorldUp) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camera.transform.position, camera.transform.position + camera.transform.up * scaledLineMagnitude);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(camera.transform.position, camera.transform.position + camera.transform.forward * scaledLineMagnitude);
            } else {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(camera.transform.position, camera.transform.position + Vector3.up * scaledLineMagnitude);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(camera.transform.position, camera.transform.position + Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized * scaledLineMagnitude);
            }
        }
    }
}
