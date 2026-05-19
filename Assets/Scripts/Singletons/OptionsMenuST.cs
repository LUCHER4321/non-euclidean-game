using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

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
    string fullscreenOnText = "Yes", fullscreenOffText = "No";
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
    public float GetSensitivity { get => (float)sensitivity / 100f; }
    public bool[] GetInverts { get => inverts; }

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
        resolutions = Screen.resolutions;
        resText.text = $"{Screen.currentResolution.width}x{Screen.currentResolution.height}";
        resSlider.maxValue = resolutions.Length - 1;
        resSlider.value = System.Array.FindIndex(resolutions, x => x.width == Screen.currentResolution.width && x.height == Screen.currentResolution.height);
        fullscreenToggle.isOn = Screen.fullScreen;
        fullscreenText.text = Screen.fullScreen ? fullscreenOnText : fullscreenOffText;
        senSlider.value = sensitivity;
        senText.text = $"{sensitivity}%";
        for (int i = 0; i < invertTogs.Length; i++) invertTogs[i].isOn = inverts[i];
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetResolution()
    {
        if (resSlider.value < 0 || resSlider.value >= resolutions.Length) return;
        Resolution res = resolutions[(int)resSlider.value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        resText.text = $"{res.width}x{res.height}";
    }

    public void SetFullscreen()
    {
        bool isFullscreen = fullscreenToggle.isOn;
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, isFullscreen);
        fullscreenText.text = isFullscreen ? fullscreenOnText : fullscreenOffText;
    }

    public void SetVolume(VolumeSlider volumeSlider)
    {
        if (volumeSlider == null) return;
        float volume = PercentToDB(volumeSlider.slider.value);
        audioMixer.SetFloat(volumeSlider.outputName, volume);
    }

    public void Controls(bool open)
    {
        optionsMenu.SetActive(!open);
        controlsMenu.SetActive(open);
    }

    public void SetSensitivity()
    {
        sensitivity = (int)senSlider.value;
        senText.text = $"{sensitivity}%";
    }

    public void SetDefaultSensitivity()
    {
        senSlider.value = defaultSensitivity;
        SetSensitivity();
    }

    public void SetInvert(int n)
    {
        inverts[n] = invertTogs[n].isOn;
    }
}
