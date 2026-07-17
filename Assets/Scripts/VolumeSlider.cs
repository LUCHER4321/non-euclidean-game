using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text volumeText;
    public string outputName;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateVolumeText(slider.value);
        slider.onValueChanged.AddListener(UpdateVolumeText);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void UpdateVolumeText(float value)
    {
        volumeText.text = $"{Mathf.RoundToInt(value)}%";
    }
}
