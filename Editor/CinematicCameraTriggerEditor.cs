using UnityEditor;
using UnityEngine;

namespace CinematicCameraSystem.EditorScripts {
    [CustomEditor(typeof(CinematicCameraTrigger)), CanEditMultipleObjects]
    public class CinematicCameraTriggerEditor : Editor {
        private SerializedProperty isStuntTriggerProperty;
        private SerializedProperty linkedStuntProperty;
        private SerializedProperty linkedCamerasProperty;
        private SerializedProperty levelShotWeightProperty;
        private SerializedProperty mountedShotWeightProperty;
        private SerializedProperty closestPointOnTrackProperty;
        private SerializedProperty distanceAlongTrackProperty;
        private SerializedProperty percentageAlongTrackProperty;
        private SerializedProperty trackDirectionProperty;
        private SerializedProperty trackPerpendicularProperty;

        private void OnEnable() {
            isStuntTriggerProperty = serializedObject.FindProperty(CinematicCameraTrigger.IS_STUNT_TRIGGER_VARIABLE_NAME);
            linkedStuntProperty = serializedObject.FindProperty(CinematicCameraTrigger.LINKED_STUNT_VARIABLE_NAME);
            linkedCamerasProperty = serializedObject.FindProperty(CinematicCameraTrigger.LINKED_CAMERAS_VARIABLE_NAME);
            levelShotWeightProperty = serializedObject.FindProperty(CinematicCameraTrigger.LEVEL_SHOT_WEIGHT_VARIABLE_NAME);
            mountedShotWeightProperty = serializedObject.FindProperty(CinematicCameraTrigger.MOUNTED_SHOT_WEIGHT_VARIABLE_NAME);
            closestPointOnTrackProperty = serializedObject.FindProperty(CinematicCameraTrigger.CLOSEST_POINT_ON_TRACK_VARIABLE_NAME);
            distanceAlongTrackProperty = serializedObject.FindProperty(CinematicCameraTrigger.DISTANCE_ALONG_TRACK_VARIABLE_NAME);
            percentageAlongTrackProperty = serializedObject.FindProperty(CinematicCameraTrigger.PERCENTAGE_ALONG_TRACK_VARIABLE_NAME);
            trackDirectionProperty = serializedObject.FindProperty(CinematicCameraTrigger.TRACK_DIRECTION_VARIABLE_NAME);
            trackPerpendicularProperty = serializedObject.FindProperty(CinematicCameraTrigger.TRACK_PERPENDICULAR_VARIABLE_NAME);
        }

        public override void OnInspectorGUI() {
            Rect levelCamerasRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Level Cameras", EditorStyles.boldLabel);
            CustomEditorHelper.DrawProperty(isStuntTriggerProperty);

            if (isStuntTriggerProperty.boolValue) {
                CustomEditorHelper.DrawProperty(linkedStuntProperty);
            } else {
                EditorGUI.indentLevel++;
                CustomEditorHelper.DrawProperty(linkedCamerasProperty);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Shot Determination", EditorStyles.boldLabel);
            if (isStuntTriggerProperty.boolValue) {
                CustomEditorHelper.DrawProperty(levelShotWeightProperty, "stuntShotWeight");
            } else {
                CustomEditorHelper.DrawProperty(levelShotWeightProperty);
            }
            CustomEditorHelper.DrawProperty(mountedShotWeightProperty);

            if (!isStuntTriggerProperty.boolValue && levelShotWeightProperty.floatValue > 0 && linkedCamerasProperty.arraySize == 0) {
                EditorGUILayout.HelpBox("WARNING: Level shot weight is above 0, but there are no linked cameras set!", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Track Values (Read Only)", EditorStyles.boldLabel);
            CustomEditorHelper.DrawPropertyReadOnlyLabel(distanceAlongTrackProperty, distanceAlongTrackProperty.floatValue.ToString());
            CustomEditorHelper.DrawPropertyReadOnlyLabel(percentageAlongTrackProperty, percentageAlongTrackProperty.floatValue.ToString("##0.0") + "%");
            CustomEditorHelper.DrawPropertyReadOnlyLabel(closestPointOnTrackProperty, closestPointOnTrackProperty.vector3Value.ToString());
            CustomEditorHelper.DrawPropertyReadOnlyLabel(trackDirectionProperty, trackDirectionProperty.vector3Value.ToString());
            CustomEditorHelper.DrawPropertyReadOnlyLabel(trackPerpendicularProperty, trackPerpendicularProperty.vector3Value.ToString());
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("Snap To Track")) {
                SnapToTrack();
            }
            EditorGUILayout.HelpBox("NOTE: The trigger's automatic snapping feature doesn't work if an inspector is in debug mode.", MessageType.Info);
        
            serializedObject.ApplyModifiedProperties();
        }

        // NOTE: OnSceneGUI is not executed if the "first" inspector window Unity finds is in debug mode.
        // Disabling debug mode on all inspectors should solve the issue.
        private void OnSceneGUI() {
            if (Event.current.type == EventType.MouseDrag || Event.current.type == EventType.Repaint) {
                SnapToTrack();
            }
        }

        private void SnapToTrack() {
            CinematicCameraTrigger script = (CinematicCameraTrigger)target;
            script.RecalculateTrackPosition();
        }

        //////////// GIZMOS ////////////
        private const float TRIGGER_GIZMO_WIDTH = 5.25f;
        private const float TRIGGER_GIZMO_HEIGHT = TRIGGER_GIZMO_WIDTH / 1.5f;
        private const float TRIGGER_GIZMO_LENGTH = TRIGGER_GIZMO_WIDTH / 24f;
        private const float CUBE_GIZMO_SCALE = 0.15f;
        private static readonly Color AZURE = new Color(0, .55f, 1f, 1f);
        private static readonly Color TRANSLUCENT_AZURE = new Color(AZURE.r, AZURE.g, AZURE.b, 0.5f);
        private static readonly Color TRANSLUCENT_YELLOW = new Color(Color.yellow.r, Color.yellow.g, Color.yellow.b, 0.25f);

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawTriggerGizmos(CinematicCameraTrigger trigger, GizmoType gizmoType) {
            Vector3 triggerGizmoCenter;
            DrawTrackAlignedTriggerCube(trigger, out triggerGizmoCenter);

            // Draw line connector to linked stunt.
            if (trigger.IsStuntTrigger) {
                if (trigger.LinkedStunt != null) {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawLine(triggerGizmoCenter, trigger.LinkedStunt.transform.position);
                }
                return;
            }

            Vector3 cubeGizmoScale = Vector3.one * CUBE_GIZMO_SCALE;

            // Draw line connectors to each linked camera.
            foreach (WeightedCinematicCamera weightedCamera in trigger.LinkedCameras) {
                CinematicCamera camera = weightedCamera?.Parameter;
                if (camera == null) { continue; }

                float handleSize = HandleUtility.GetHandleSize(camera.transform.position);

                Gizmos.color = AZURE;
                Gizmos.DrawLine(triggerGizmoCenter, camera.transform.position);
                Gizmos.color = TRANSLUCENT_AZURE;
                Gizmos.DrawCube(camera.transform.position, cubeGizmoScale * handleSize);
            }
        }

        private static void DrawTrackAlignedTriggerCube(CinematicCameraTrigger trigger, out Vector3 triggerGizmoCenter) {
            Vector3 trackUp = Vector3.Cross(trigger.TrackDirection, trigger.TrackPerpendicular);
            Vector3 cubeScale = new Vector3(TRIGGER_GIZMO_WIDTH, TRIGGER_GIZMO_HEIGHT, TRIGGER_GIZMO_LENGTH);
            triggerGizmoCenter = trigger.ClosestPointOnTrack + trackUp * cubeScale.y / 2f;

            Matrix4x4 originalMatrix = Gizmos.matrix;
            Matrix4x4 matrix = new Matrix4x4();

            matrix.SetColumn(0, trigger.TrackPerpendicular);
            matrix.SetColumn(1, trackUp);
            matrix.SetColumn(2, trigger.TrackDirection);
            matrix.SetColumn(3, triggerGizmoCenter);
            Gizmos.matrix = matrix;

            Gizmos.color = trigger.IsStuntTrigger ? TRANSLUCENT_YELLOW : TRANSLUCENT_AZURE;
            Gizmos.DrawCube(Vector3.zero, cubeScale);
            Gizmos.color = trigger.IsStuntTrigger ? Color.yellow : AZURE;
            Gizmos.DrawWireCube(Vector3.zero, cubeScale);

            Gizmos.matrix = originalMatrix;
        }
    }
}
