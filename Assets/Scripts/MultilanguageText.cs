using UnityEngine;
using TMPro;

public class MultilanguageText : MonoBehaviour
{
    [SerializeField]
    TMP_Text text;
    [SerializeField]
    LanText str;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeLanguage();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeLanguage()
    {
        text.text = str.GetText();
    }
}
