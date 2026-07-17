using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Bake3DNoise))]
public class Bake3DNoiseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Bake3DNoise bakeScript = (Bake3DNoise)target;
        GUILayout.Space(10);
        if (GUILayout.Button("Bake 3D Texture")) bakeScript.BakeTexture();
    }
}
