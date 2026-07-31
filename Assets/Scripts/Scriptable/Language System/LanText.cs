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

    public string GetText(params string[] stringParams)
    {
        if (OptionsMenuST.Instance == null) return "";
        foreach (Translation t in translations)
        {
            if (t.lan == OptionsMenuST.Instance.GetLanguage)
            {
                string txt = t.text;
                for (int i = 0; i < stringParams.Length; i++) txt = txt.Replace("{" + i.ToString() + "}", stringParams[i]);
                return txt;
            }
        }
        return "";
    }
}
