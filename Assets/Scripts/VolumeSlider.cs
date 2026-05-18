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

    }

    // Update is called once per frame
    void Update()
    {
        volumeText.text = $"{Mathf.RoundToInt(slider.value)}%";
    }
}
