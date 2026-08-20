using UnityEngine;
using UnityEditor;

public abstract class DistributionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        int lineCount = 2;
        SerializedProperty distTypeProp = property.FindPropertyRelative("distType");
        lineCount += GetAdditionalLineCount(distTypeProp.enumValueIndex);
        return (EditorGUIUtility.singleLineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        Rect currentRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        property.isExpanded = EditorGUI.Foldout(currentRect, property.isExpanded, label);
        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty distTypeProp = property.FindPropertyRelative("distType");
            EditorGUI.PropertyField(currentRect, distTypeProp);
            currentRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            DrawDistributionFields(ref currentRect, property, distTypeProp.enumValueIndex);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }

    protected void DrawField(ref Rect rect, SerializedProperty parentProperty, string fieldName)
    {
        SerializedProperty fieldProperty = parentProperty.FindPropertyRelative(fieldName);
        EditorGUI.PropertyField(rect, fieldProperty);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    protected abstract int GetAdditionalLineCount(int enumValueIndex);
    protected abstract void DrawDistributionFields(ref Rect rect, SerializedProperty property, int enumValueIndex);
}