using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MusicVolumeController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer; // Exposed to Inspector
    [SerializeField] private Slider volumeSlider;   // Exposed to Inspector

    void Start()
    {
        float volume;
        audioMixer.GetFloat("MusicVolume", out volume);
        volumeSlider.value = Mathf.Pow(10, volume / 20); // Convert dB to linear scale

        volumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    public void SetMusicVolume(float value)
    {
        // Logarithmic conversion to handle volume scaling
        if (value <= 0.001f) // Prevent errors for zero values
        {
            audioMixer.SetFloat("MusicVolume", -80f); // Mute
        }
        else
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(value) * 20); // Adjust volume
        }
    }

}
