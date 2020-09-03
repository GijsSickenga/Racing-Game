using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides helper functions for custom editors.
/// </summary>
public static class CustomEditorHelper {
    /// <summary>
    /// Draws the given property.
    /// </summary>
    private static void DrawPropertyInternal(SerializedProperty property, GUIContent content) {
        EditorGUILayout.PropertyField(property, content, true);
    }
    
    /// <summary>
    /// Draws the given property as it would be drawn in a normal inspector window.
    /// </summary>
    public static void DrawProperty(SerializedProperty property, string variableNameOverride = null) {
        string label = variableNameOverride ?? property.name;
        DrawPropertyInternal(property, new GUIContent(ObjectNames.NicifyVariableName(label)));
    }
    /// <summary>
    /// Draws the given property as it would be drawn in a normal inspector window, including a custom ToolTip.
    /// </summary>
    public static void DrawPropertyWithTooltip(SerializedProperty property, string tooltip, string variableNameOverride = null) {
        string label = variableNameOverride ?? property.name;
        DrawPropertyInternal(property, new GUIContent(ObjectNames.NicifyVariableName(label), tooltip));
    }

    /// <summary>
    /// Draws the given property as read-only text.
    /// Pass in the property's value as a string for proper formatting, such as property.floatValue.ToString().
    /// </summary>
    public static void DrawPropertyReadOnlyLabel(SerializedProperty property, string propertyValueText) {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(ObjectNames.NicifyVariableName(property.name));
        EditorGUILayout.SelectableLabel(propertyValueText, GUILayout.Height(EditorGUI.GetPropertyHeight(property)));
        EditorGUILayout.EndHorizontal();
    }
}
