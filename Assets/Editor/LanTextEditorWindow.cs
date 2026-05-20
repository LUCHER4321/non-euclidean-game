using UnityEditor;
using UnityEngine;

public class LanTextEditorWindow : EditorWindow
{
    private Language[] languages;
    private LanText[] lanTexts;
    private Vector2 scrollPosition;
    private readonly float nameColWidth = 150f;
    private readonly float contextColWidth = 250f;
    private readonly float langColWidth = 200f;

    [MenuItem("Tools/Translations Editor (LanText)")]
    public static void ShowWindow()
    {
        GetWindow<LanTextEditorWindow>("LanText Table");
    }

    private void OnEnable()
    {
        LoadData();
    }

    private void LoadData()
    {
        languages = Resources.LoadAll<Language>("");
        lanTexts = Resources.LoadAll<LanText>("");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Refresh Data", GUILayout.Height(30))) LoadData();
        if (languages == null || languages.Length == 0)
        {
            EditorGUILayout.HelpBox("Objects 'Language' not Found in the Resources Folder.", MessageType.Warning);
            return;
        }
        if (lanTexts == null || lanTexts.Length == 0)
        {
            EditorGUILayout.HelpBox("Objects 'LanText' not Found in the Resources Folder.", MessageType.Warning);
            return;
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        DrawHeaders();
        DrawRows();
        GUILayout.EndScrollView();
    }

    private void DrawHeaders()
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("LanText Name", EditorStyles.boldLabel, GUILayout.Width(nameColWidth));
        GUILayout.Label("Context", EditorStyles.boldLabel, GUILayout.Width(contextColWidth));
        foreach (Language lang in languages) GUILayout.Label(lang.GetName, EditorStyles.boldLabel, GUILayout.Width(langColWidth));
        GUILayout.EndHorizontal();
    }

    private void DrawRows()
    {
        foreach (LanText lanText in lanTexts)
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label(lanText.name, EditorStyles.label, GUILayout.Width(nameColWidth));
            SerializedObject so = new SerializedObject(lanText);
            so.Update();
            SerializedProperty contextProp = so.FindProperty("context");
            if (contextProp != null) contextProp.stringValue = EditorGUILayout.TextArea(contextProp.stringValue, GUILayout.Width(contextColWidth), GUILayout.MinHeight(40));
            SerializedProperty translationsProp = so.FindProperty("translations");
            if (translationsProp != null) foreach (Language lang in languages) DrawTranslationField(translationsProp, lang);
            so.ApplyModifiedProperties();
            GUILayout.EndHorizontal();
        }
    }

    private void DrawTranslationField(SerializedProperty translationsProp, Language targetLang)
    {
        SerializedProperty textProp = null;
        for (int i = 0; i < translationsProp.arraySize; i++)
        {
            SerializedProperty element = translationsProp.GetArrayElementAtIndex(i);
            SerializedProperty lanProp = element.FindPropertyRelative("lan");
            if (lanProp != null && lanProp.objectReferenceValue == targetLang)
            {
                textProp = element.FindPropertyRelative("text");
                break;
            }
        }
        if (textProp != null) textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Width(langColWidth), GUILayout.MinHeight(40));
        else if (GUILayout.Button("+ Add " + targetLang.GetName, GUILayout.Width(langColWidth), GUILayout.Height(40)))
        {
            int newIndex = translationsProp.arraySize;
            translationsProp.InsertArrayElementAtIndex(newIndex);
            SerializedProperty newElement = translationsProp.GetArrayElementAtIndex(newIndex);
            newElement.FindPropertyRelative("lan").objectReferenceValue = targetLang;
            newElement.FindPropertyRelative("text").stringValue = "";
        }
    }
}
