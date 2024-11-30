using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;

    public void SetVolume(float volume)
    {
        if (volume <= 0.01f) // Slider at or near 0
        {
            audioMixer.SetFloat("MasterVolume", -80f); // Mute by setting to minimum dB
        }
        else
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20); // Convert to dB scale
        }
    }

}
