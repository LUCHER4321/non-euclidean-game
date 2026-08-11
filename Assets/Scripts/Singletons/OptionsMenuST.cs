using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Linq;

public class OptionsMenuST : MonoBehaviour
{
    public static OptionsMenuST Instance { get; private set; }
    [Header("Sections")]
    [SerializeField]
    GameObject optionsMenu;
    [SerializeField]
    GameObject controlsMenu;
    [Header("Resolution")]
    [SerializeField]
    Slider resSlider;
    [SerializeField]
    TMP_Text resText;
    [Header("Fullscreen")]
    [SerializeField]
    Toggle fullscreenToggle;
    [SerializeField]
    TMP_Text fullscreenText;
    [SerializeField]
    LanText fullscreenOnText, fullscreenOffText;
    [Header("Audio")]
    [SerializeField]
    AudioMixer audioMixer;
    [Header("Player")]
    [SerializeField]
    Slider senSlider;
    [SerializeField]
    TMP_Text senText;
    [SerializeField, Range(0, 100)]
    int sensitivity = 50, defaultSensitivity = 50;
    [SerializeField]
    Toggle[] invertTogs = new Toggle[2];
    [SerializeField]
    bool[] inverts = new bool[] { false, false };
    private Resolution[] resolutions;
    [Header("Language")]
    private Language[] languages;
    [SerializeField]
    Language language;
    [SerializeField]
    TMP_Dropdown lanDropdown;
    public float GetSensitivity { get => (float)sensitivity / 100f; }
    public bool[] GetInverts { get => inverts; }
    public Language GetLanguage { get => language; }

    private const string PREF_RESOLUTION = "ResIndex",
    PREF_FULLSCREEN = "Fullscreen",
    PREF_SENSITIVITY = "Sensitivity",
    PREF_INVERT_X = "InvertX",
    PREF_INVERT_Y = "InvertY",
    PREF_LANGUAGE = "Language",
    PREF_VOLUME = "Volume_";
    private const string[] PREF_INVERT = new string[] { PREF_INVERT_X, PREF_INVERT_Y };

    static float PercentToDB(float p)
    {
        return 20 * Mathf.Log10(p / 100f);
    }

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        languages = Resources.LoadAll<Language>("");
        lanDropdown.AddOptions(new List<string>(languages.Select(x => x.GetName)));
        lanDropdown.value = System.Array.FindIndex(lanDropdown.options.ToArray(), x => x.text == language.GetName);
        resolutions = Screen.resolutions;
        resText.text = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        resSlider.maxValue = resolutions.Length - 1;
        resSlider.value = System.Array.FindIndex(resolutions, x => x.width == Screen.currentResolution.width && x.height == Screen.currentResolution.height);
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenText.text = (Screen.fullScreen ? fullscreenOnText : fullscreenOffText).GetText();
        senSlider.value = sensitivity;
        senText.text = $"{sensitivity}%";
        for (int i = 0; i < invertTogs.Length; i++) invertTogs[i].isOn = inverts[i];
        LoadSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LoadSettings()
    {
        resSlider.value = PlayerPrefs.HasKey(PREF_RESOLUTION) ? PlayerPrefs.GetInt(PREF_RESOLUTION) : System.Array.FindIndex(resolutions, x => x.width == Screen.currentResolution.width && x.height == Screen.currentResolution.height);
        SetResolution();
        int defaultFullscreen = Screen.fullScreen ? 1 : 0;
        fullscreenToggle.isOn = PlayerPrefs.GetInt(PREF_FULLSCREEN, defaultFullscreen) == 1;
        SetFullscreen();
        sensitivity = PlayerPrefs.GetInt(PREF_SENSITIVITY, defaultSensitivity);
        senSlider.value = sensitivity;
        SetSensitivity();
        for (int i = 0; i < invertTogs.Length; i++)
        {
            inverts[i] = PlayerPrefs.GetInt(PREF_INVERT[i], 0) == 1;
            invertTogs[i].isOn = inverts[i];
        }
        string savedLanguage = PlayerPrefs.GetString(PREF_LANGUAGE, language.GetName);
        int langIndex = System.Array.FindIndex(lanDropdown.options.ToArray(), x => x.text == savedLanguage);
        lanDropdown.value = langIndex >= 0 ? langIndex : 0;
        ChangeLanguage();
    }

    public void SetResolution()
    {
        if (resSlider.value < 0 || resSlider.value >= resolutions.Length) return;
        Resolution res = resolutions[(int)resSlider.value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        resText.text = $"{res.width}x{res.height}";
        PlayerPrefs.SetInt(PREF_RESOLUTION, (int)resSlider.value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen()
    {
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);
        fullscreenText.text = (isFullscreen ? fullscreenOnText : fullscreenOffText).GetText();
        PlayerPrefs.SetInt(PREF_FULLSCREEN, isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolume(VolumeSlider volumeSlider)
    {
        if (volumeSlider == null) return;
        float volume = PercentToDB(volumeSlider.slider.value);
        audioMixer.SetFloat(volumeSlider.outputName, volume);
        PlayerPrefs.SetFloat(PREF_VOLUME + volumeSlider.outputName, volumeSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void Controls(bool open)
    {
        optionsMenu.SetActive(!open);
        controlsMenu.SetActive(open);
    }

    public void RestoreControls()
    {
        foreach (RebindableAction r in FindObjectsByType<RebindableAction>(FindObjectsInactive.Include, FindObjectsSortMode.None)) r.ResetToDefault();
    }

    public void SetSensitivity()
    {
        sensitivity = (int)senSlider.value;
        senText.text = $"{sensitivity}%";
        PlayerPrefs.SetInt(PREF_SENSITIVITY, sensitivity);
        PlayerPrefs.Save();
    }

    public void SetDefaultSensitivity()
    {
        senSlider.value = defaultSensitivity;
        SetSensitivity();
    }

    public void SetInvert(int n)
    {
        inverts[n] = invertTogs[n].isOn;
        PlayerPrefs.SetInt(PREF_INVERT[n], inverts[n] ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ChangeLanguage()
    {
        language = languages.First(x => x.GetName == lanDropdown.options[lanDropdown.value].text);
        foreach (MultilanguageText mlt in FindObjectsByType<MultilanguageText>(FindObjectsInactive.Include, FindObjectsSortMode.None)) mlt.ChangeLanguage();
        SetFullscreen();
        PlayerPrefs.SetString(PREF_LANGUAGE, language.GetName);
        PlayerPrefs.Save();
    }
}
