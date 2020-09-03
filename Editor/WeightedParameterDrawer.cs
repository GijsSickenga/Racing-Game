using CinematicCameraSystem;
using UnityEditor;
using UnityEngine;

// NOTE: Add CustomPropertyDrawers for subclasses of WeightedParameter at the bottom of the file.

namespace WeightedRandomization.EditorScripts {
    public class WeightedParameterDrawer<T> : PropertyDrawer {
        private static readonly GUIContent WEIGHT_LABEL = new GUIContent("W", "Weight");

        // Pixel dimensions.
        private const int HORIZONTAL_SPACING = 5;
        private const int FIRST_LINE_HEIGHT = 16;
        private const int SECOND_LINE_HEIGHT = 18;
        private const int SINGLE_LETTER_LABEL_WIDTH = 16;
        private const int MIN_INSPECTOR_WIDTH_FOR_SINGLE_LINE = 390;

        // Scalars.
        private const float WEIGHT_PORTION_OF_WIDTH = 0.2f;
        private const float PARAMETER_PORTION_OF_WIDTH_SCALAR = (1f - WEIGHT_PORTION_OF_WIDTH) / WEIGHT_PORTION_OF_WIDTH;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            int previousIndentLevel = EditorGUI.indentLevel;
            label = EditorGUI.BeginProperty(position, label, property);
            Rect contentRect = EditorGUI.PrefixLabel(position, label);
            
            // Use two lines if necessary.
            if (position.height > FIRST_LINE_HEIGHT) {
                position.height = FIRST_LINE_HEIGHT;
                EditorGUI.indentLevel += 1;
                contentRect = EditorGUI.IndentedRect(position);
                contentRect.y += SECOND_LINE_HEIGHT;
            }

            contentRect.width *= WEIGHT_PORTION_OF_WIDTH;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = SINGLE_LETTER_LABEL_WIDTH;
            EditorGUI.PropertyField(contentRect, property.FindPropertyRelative(WeightedParameter<T>.WEIGHT_VARIABLE_NAME), WEIGHT_LABEL);

            contentRect.x += contentRect.width + HORIZONTAL_SPACING;
            contentRect.width = contentRect.width * PARAMETER_PORTION_OF_WIDTH_SCALAR - HORIZONTAL_SPACING;
            SerializedProperty parameterProperty = property.FindPropertyRelative(WeightedParameter<T>.PARAMETER_VARIABLE_NAME);

            // Use first letter of parameter type as the label for the parameter. Type name starts at index 6 of "PPtr<$TYPE_NAME>".
            string parameterTypeName = parameterProperty.type.Substring(6, parameterProperty.type.Length - 7);
            EditorGUI.PropertyField(contentRect, parameterProperty, new GUIContent(parameterTypeName.Substring(0, 1), parameterTypeName));
            
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = previousIndentLevel;
        }

        // Gives the property more space if it needs to fit its contents on two lines.
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            // Screen.width provides the width of the inspector when called here.
            bool twoLinesRequired = Screen.width < MIN_INSPECTOR_WIDTH_FOR_SINGLE_LINE && label != GUIContent.none;
            return twoLinesRequired ? (FIRST_LINE_HEIGHT + SECOND_LINE_HEIGHT) : FIRST_LINE_HEIGHT;
        }
    }

    ////////// NOTE: Add CustomPropertyDrawers for subclasses of WeightedParameter here. //////////
    //////////       This allows them to be properly displayed in the inspector.         //////////
    //////////       Make sure the subclasses are marked serializable.                   //////////

    [CustomPropertyDrawer(typeof(WeightedCinematicCamera))]
    public class WeightedCinematicCameraDrawer : WeightedParameterDrawer<CinematicCamera> { }

    [CustomPropertyDrawer(typeof(MountedCameraRig.WeightedMountedCamera))]
    public class WeightedMountedCameraDrawer : WeightedParameterDrawer<MountedCamera> { }

    [CustomPropertyDrawer(typeof(WeightedCinematicShotType))]
    public class WeightedCinematicShotTypeDrawer : WeightedParameterDrawer<CinematicShotType> { }
}
