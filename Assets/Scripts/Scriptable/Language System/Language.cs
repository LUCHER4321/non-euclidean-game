using UnityEngine;

[CreateAssetMenu(fileName = "Language", menuName = "Scriptable Objects/Language System/Language")]
public class Language : ScriptableObject
{
    [SerializeField]
    string code;
    [SerializeField]
    string lanName;
    public string GetCode { get => code; }
    public string GetName { get => lanName; }
}
