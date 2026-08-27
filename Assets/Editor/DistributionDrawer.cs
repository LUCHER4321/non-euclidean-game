using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

public abstract class DistributionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded) return EditorGUIUtility.singleLineHeight;
        int lineCount = 4;
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
            DrawReadOnlyStats(ref currentRect, property);
            EditorGUI.indentLevel--;
        }
        EditorGUI.EndProperty();
    }

    private void DrawReadOnlyStats(ref Rect rect, SerializedProperty property)
    {
        IDistribution distribution = GetTargetObjectOfProperty(property) as IDistribution;
        if (distribution != null)
        {
            EditorGUI.BeginDisabledGroup(true);
            float expectancy = distribution.Expectancy();
            float stdDev = distribution.StandardDeviation();
            EditorGUI.FloatField(rect, "Expectancy", expectancy);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.FloatField(rect, "Standard Dev", stdDev);
            rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.EndDisabledGroup();
        }
    }

    protected void DrawField(ref Rect rect, SerializedProperty parentProperty, string fieldName)
    {
        SerializedProperty fieldProperty = parentProperty.FindPropertyRelative(fieldName);
        EditorGUI.PropertyField(rect, fieldProperty);
        rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
    }

    protected abstract int GetAdditionalLineCount(int enumValueIndex);
    protected abstract void DrawDistributionFields(ref Rect rect, SerializedProperty property, int enumValueIndex);

    private object GetTargetObjectOfProperty(SerializedProperty prop)
    {
        var path = prop.propertyPath.Replace(".Array.data[", "[");
        object obj = prop.serializedObject.targetObject;
        var elements = path.Split('.');
        foreach (var element in elements)
        {
            if (element.Contains("["))
            {
                var elementName = element.Substring(0, element.IndexOf("["));
                var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                obj = GetValue_Imp(obj, elementName, index);
            }
            else
            {
                obj = GetValue_Imp(obj, element);
            }
        }
        return obj;
    }

    private object GetValue_Imp(object source, string name)
    {
        if (source == null) return null;
        var type = source.GetType();
        var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (f == null)
        {
            var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return null;
            return p.GetValue(source, null);
        }
        return f.GetValue(source);
    }

    private object GetValue_Imp(object source, string name, int index)
    {
        var enumerable = GetValue_Imp(source, name) as IEnumerable;
        if (enumerable == null) return null;
        var enm = enumerable.GetEnumerator();
        for (int i = 0; i <= index; i++) if (!enm.MoveNext()) return null;
        return enm.Current;
    }
}