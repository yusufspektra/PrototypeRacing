#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SpektraGames.SpektraUtilities.Runtime
{
    [CustomPropertyDrawer(typeof(AutoResizeMultilineAttribute))]
    public class AutoResizeMultilineDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            AutoResizeMultilineAttribute autoResize = (AutoResizeMultilineAttribute)attribute;

            // Split the string into lines based on newlines
            int lineCount = Mathf.Clamp(property.stringValue.Split('\n').Length, autoResize.MinLines, autoResize.MaxLines);

            // Calculate the total height of the field
            return EditorGUIUtility.singleLineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            AutoResizeMultilineAttribute autoResize = (AutoResizeMultilineAttribute)attribute;

            // Draw label and get the text area position
            position = EditorGUI.PrefixLabel(position, label);
            EditorGUI.BeginProperty(position, label, property);

            // Draw text area with dynamic height
            property.stringValue = EditorGUI.TextArea(position, property.stringValue);
        
            EditorGUI.EndProperty();
        }
    }
}
#endif   
