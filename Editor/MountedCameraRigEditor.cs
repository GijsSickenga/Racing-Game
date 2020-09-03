using UnityEditor;
using UnityEngine;

namespace CinematicCameraSystem.EditorScripts {
    [CustomEditor(typeof(MountedCameraRig))]
    public class MountedCameraRigEditor : Editor {
        private MountedCameraRig Script { get { return (MountedCameraRig)target; } }

        private void OnSceneGUI() {
            Event e = Event.current;
            if (e.type == EventType.ExecuteCommand) {
                // Look at followTarget rather than rig transform if followTarget is set.
                BaseVehicleDriver followTarget = serializedObject.FindProperty(MountedCameraRig.FOLLOW_TARGET_VARIABLE_NAME)?.objectReferenceValue as BaseVehicleDriver;
                if (followTarget == null) { return; }
                
                if (e.commandName == "FrameSelected") {
                    SceneView.lastActiveSceneView.LookAt(followTarget.transform.position);
                    e.Use();
                } else if (e.commandName == "FrameSelectedWithLock") {
                    Selection.objects = new Object[] { followTarget.gameObject, Script.gameObject };
                    SceneView.lastActiveSceneView.FrameSelected(true);
                    e.Use();
                }
            }
        }

        //////////// GIZMOS ////////////
        private const float CAR_SCALE = 0.2f;
        private const float CAR_WIDTH = CAR_SCALE;
        private const float CAR_HEIGHT = CAR_SCALE * 0.65f;
        private const float CAR_LENGTH = CAR_SCALE * 2.5f;
        private const float WHEEL_SCALE = CAR_SCALE / 4f;
        private const float WHEELS_X_OFFSET = CAR_WIDTH / 2f;
        private const float WHEELS_Z_OFFSET = (CAR_LENGTH - (WHEEL_SCALE * 3.5f)) / 2f;

        private const float TRACK_WIDTH = 5f;
        private const float HALF_TRACK_WIDTH = TRACK_WIDTH / 2f;

        private const float DOT_SIZE = 5f;

        private static readonly Color TRANSLUCENT_YELLOW = new Color(1, 0.8f, 0, 0.025f);
        private static readonly Color AZURE = new Color(0, .55f, 1f, 1f);
        private static readonly Color TRANSLUCENT_AZURE = new Color(AZURE.r, AZURE.g, AZURE.b, 0.5f);

        [DrawGizmo(GizmoType.InSelectionHierarchy)]
        private static void DrawMountedRigGizmos(MountedCameraRig rig, GizmoType gizmoType) {
            Transform rigCenter = (rig.FollowVehicleViewTransform ?? rig.FollowVehicle?.transform) ?? rig.transform;
            Vector3 offsetFromTrackCenter = (Vector3.right * rig.FollowVehicle?.Controller?.ClosestPathDistanceToPoint) ?? Vector3.zero;

            // Align gizmos with rig transform by moving to local space.
            Matrix4x4 originalMatrix = Handles.matrix;
            Handles.matrix = rigCenter.localToWorldMatrix;

            // Rig car.
            DrawTargetingGizmos(Vector3.zero, offsetFromTrackCenter, rig);
            DrawCarGizmos(Vector3.zero, rigCenter.up, rigCenter.right, AZURE);

            // Lookat target.
            if (rig.LookAtVehicle != null && rig.FollowVehicle != null && Application.isPlaying) {
                Vector3 relativeLookAtPosition = TrackHelper.GetRelativePosition3D(rig.LookAtVehicle, rig.FollowVehicle);
                Handles.color = Color.white;
                Handles.DrawDottedLine(Vector3.zero, relativeLookAtPosition, DOT_SIZE);
                DrawCarGizmos(relativeLookAtPosition, rigCenter.transform.up, rigCenter.transform.right, Color.white);
            }

            Handles.matrix = originalMatrix;
        }

        private static void DrawTargetingGizmos(Vector3 rigCenter, Vector3 offsetFromTrackCenter, MountedCameraRig rig) {
            Vector3 trackCenter = rigCenter - offsetFromTrackCenter;

            Vector3 trackWidthOffset = new Vector3(HALF_TRACK_WIDTH, 0, 0);
            Vector3 minDistanceOffset = new Vector3(0, 0, rig.MinRelativeDistance);
            Vector3 idealDistanceOffset = new Vector3(0, 0, rig.IdealRelativeDistance);
            Vector3 maxDistanceOffset = new Vector3(0, 0, rig.MaxRelativeDistance);

            // Distances.
            Handles.color = Color.yellow;
            DrawDistanceLines(trackCenter, minDistanceOffset, trackWidthOffset);
            Handles.color = Color.green;
            DrawDottedDistanceLines(trackCenter, idealDistanceOffset, trackWidthOffset, DOT_SIZE);
            Handles.color = Color.yellow;
            DrawDistanceLines(trackCenter, maxDistanceOffset, trackWidthOffset);

            // Track edges within targeting area.
            Vector3 frontRightCornerOffset = maxDistanceOffset + trackWidthOffset;
            Vector3 frontLeftCornerOffset = maxDistanceOffset - trackWidthOffset;
            Handles.color = AZURE;
            Handles.DrawLine(trackCenter + frontRightCornerOffset, trackCenter - frontLeftCornerOffset);
            Handles.DrawLine(trackCenter + frontLeftCornerOffset, trackCenter - frontRightCornerOffset);
            // Track edges beyond max distance.
            Handles.DrawDottedLine(trackCenter + frontRightCornerOffset, trackCenter + frontRightCornerOffset + Vector3.forward * HALF_TRACK_WIDTH, DOT_SIZE);
            Handles.DrawDottedLine(trackCenter + frontLeftCornerOffset, trackCenter + frontLeftCornerOffset + Vector3.forward * HALF_TRACK_WIDTH, DOT_SIZE);
            Handles.DrawDottedLine(trackCenter - frontRightCornerOffset, trackCenter - frontRightCornerOffset - Vector3.forward * HALF_TRACK_WIDTH, DOT_SIZE);
            Handles.DrawDottedLine(trackCenter - frontLeftCornerOffset, trackCenter - frontLeftCornerOffset - Vector3.forward * HALF_TRACK_WIDTH, DOT_SIZE);

            // Angle cull area.
            float dotGizmoRadius = Mathf.Sqrt((rig.MaxRelativeDistance * rig.MaxRelativeDistance) + (HALF_TRACK_WIDTH * HALF_TRACK_WIDTH));
            float cullArcAngle = rig.CullAngle * 2f;
            Vector3 arcStartVector = Quaternion.Euler(0, -rig.CullAngle, 0) * Vector3.forward;
            Handles.color = TRANSLUCENT_YELLOW;
            DrawAngleArcs(rigCenter, arcStartVector, cullArcAngle, dotGizmoRadius);
            Handles.color = Color.yellow;
            DrawWireAngleArcs(rigCenter, arcStartVector, cullArcAngle, dotGizmoRadius);

            // Ideal angle.
            Vector3 idealDirection = Quaternion.Euler(0, -rig.IdealAngle, 0) * Vector3.forward;
            Handles.color = Color.green;
            DrawDottedAngleLines(rigCenter, idealDirection, dotGizmoRadius, DOT_SIZE);
            DrawDottedAngleLines(rigCenter, -idealDirection, dotGizmoRadius, DOT_SIZE);

            // Ideal car positions.
            Vector3 idealCarPosition = new Vector3(idealDirection.x / idealDirection.z * rig.IdealRelativeDistance, 0, rig.IdealRelativeDistance);
            Vector3 reflectedIdealCarPosition = Vector3.Reflect(-idealCarPosition, Vector3.forward);
            DrawCarGizmos(rigCenter + idealCarPosition, Vector3.up, Vector3.right, Color.green);
            DrawCarGizmos(rigCenter + reflectedIdealCarPosition, Vector3.up, Vector3.right, Color.green);
            DrawCarGizmos(rigCenter - idealCarPosition, Vector3.up, Vector3.right, Color.green);
            DrawCarGizmos(rigCenter - reflectedIdealCarPosition, Vector3.up, Vector3.right, Color.green);
        }

        private static void DrawCarGizmos(Vector3 center, Vector3 up, Vector3 right, Color color) {
            Handles.color = color;
            // Body.
            Handles.DrawWireCube(center + up * CAR_HEIGHT * 0.15f, new Vector3(CAR_WIDTH, CAR_HEIGHT, CAR_LENGTH));
            // Wheels.
            Vector3 wheelCenter = center - up * CAR_HEIGHT * 0.15f;
            Handles.DrawWireDisc(wheelCenter + new Vector3(WHEELS_X_OFFSET, 0, WHEELS_Z_OFFSET), right, WHEEL_SCALE);
            Handles.DrawWireDisc(wheelCenter + new Vector3(WHEELS_X_OFFSET, 0, -WHEELS_Z_OFFSET), right, WHEEL_SCALE);
            Handles.DrawWireDisc(wheelCenter + new Vector3(-WHEELS_X_OFFSET, 0, WHEELS_Z_OFFSET), -right, WHEEL_SCALE);
            Handles.DrawWireDisc(wheelCenter + new Vector3(-WHEELS_X_OFFSET, 0, -WHEELS_Z_OFFSET), -right, WHEEL_SCALE);
        }

        private static void DrawDistanceLines(Vector3 center, Vector3 distanceOffset, Vector3 trackWidthOffset) {
            Handles.DrawLine(center + distanceOffset + trackWidthOffset, center + distanceOffset - trackWidthOffset);
            Handles.DrawLine(center - distanceOffset + trackWidthOffset, center - distanceOffset - trackWidthOffset);
        }

        private static void DrawDottedDistanceLines(Vector3 center, Vector3 distanceOffset, Vector3 trackWidthOffset, float dotSize) {
            Handles.DrawDottedLine(center + distanceOffset + trackWidthOffset, center + distanceOffset - trackWidthOffset, dotSize);
            Handles.DrawDottedLine(center - distanceOffset + trackWidthOffset, center - distanceOffset - trackWidthOffset, dotSize);
        }

        private static void DrawAngleArcs(Vector3 center, Vector3 arcStartVector, float angle, float radius) {
            Handles.DrawSolidArc(center, Vector3.up, arcStartVector, angle, radius);
            Handles.DrawSolidArc(center, Vector3.up, -arcStartVector, angle, radius);
        }

        private static void DrawWireAngleArcs(Vector3 center, Vector3 arcStartDirection, float angle, float radius) {
            Handles.DrawWireArc(center, Vector3.up, arcStartDirection, angle, radius);
            DrawAngleLines(center, arcStartDirection, radius);

            Handles.DrawWireArc(center, Vector3.up, -arcStartDirection, angle, radius);
            DrawAngleLines(center, -arcStartDirection, radius);
        }

        private static void DrawAngleLines(Vector3 center, Vector3 direction, float radius) {
            Vector3 scaledDirection = direction * radius;
            Vector3 reflectedScaledDirection = Vector3.Reflect(-scaledDirection, Vector3.forward);

            Handles.DrawLine(center, center + scaledDirection);
            Handles.DrawLine(center, center + reflectedScaledDirection);
        }

        private static void DrawDottedAngleLines(Vector3 center, Vector3 direction, float radius, float dotSize) {
            Vector3 scaledDirection = direction * radius;
            Vector3 reflectedScaledDirection = Vector3.Reflect(-scaledDirection, Vector3.forward);

            Handles.DrawDottedLine(center, center + scaledDirection, dotSize);
            Handles.DrawDottedLine(center, center + reflectedScaledDirection, dotSize);
        }
    }
}
