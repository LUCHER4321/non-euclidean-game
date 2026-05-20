using UnityEngine;

[CreateAssetMenu(fileName = "LanText", menuName = "Scriptable Objects/Language System/Language Text")]
public class LanText : ScriptableObject
{
    [System.Serializable]
    struct Translation
    {
        public Language lan;
        public string text;
    }
    [SerializeField, TextArea]
    string context;
    [SerializeField]
    Translation[] translations;
    public string GetContext { get => context; }

    public string GetText()
    {
        if (OptionsMenuST.Instance == null) return "";
        foreach (Translation t in translations) if (t.lan == OptionsMenuST.Instance.GetLanguage) return t.text;
        return "";
    }
}
