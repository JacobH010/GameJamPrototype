using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class O2TankState : MonoBehaviour
{
    [SerializeField, Tooltip("Indicates if the tank is flagged as active")]
    private bool isActive = false;

    [SerializeField, Tooltip("Sound to play when the tank becomes active")]
    private AudioClip activeSound;

    [SerializeField, Tooltip("Sound to play when the tank becomes inactive")]
    private AudioClip inactiveSound;

    [SerializeField, Tooltip("Audio Mixer Group for sound effects")]
    private AudioMixerGroup effectsMixerGroup;

    private AudioSource audioSource;

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive != value) // Only act if the value changes
            {
                isActive = value;
                PlayStateChangeSound(); // Play the appropriate sound
                if (isActive)
                {
                    AssignSliderToUIManager();
                }
            }
        }
    }

    private void Awake()
    {
        // Ensure there is an AudioSource component on this GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Assign the AudioMixerGroup
        if (effectsMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = effectsMixerGroup;
        }
    }

    private void PlayStateChangeSound()
    {
        if (audioSource == null) return;

        // Play the appropriate sound based on the new state
        if (isActive && activeSound != null)
        {
            audioSource.PlayOneShot(activeSound);
        }
        else if (!isActive && inactiveSound != null)
        {
            audioSource.PlayOneShot(inactiveSound);
        }
    }

    private void AssignSliderToUIManager()
    {
        // Find the child Slider component
        Slider childSlider = GetComponentInChildren<Slider>();
        if (childSlider != null)
        {
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                uiManager.o2Slider = childSlider; // Assign the slider to the UIManager
                uiManager.UpdateO2FromSlider(); // Notify the UIManager to update the O2 value
                Debug.Log($"Assigned slider from {gameObject.name} to UIManager.");
            }
            else
            {
                Debug.LogError("UIManager not found in the scene.");
            }
        }
        else
        {
            Debug.LogError($"No Slider component found on {gameObject.name} or its children.");
        }
    }
}
