using UnityEngine;
using UnityEngine.UI;

public class SliderImageMask : MonoBehaviour
{
    public Slider slider;           // Reference to the Slider
    public RectTransform maskRect; // RectTransform of the MaskContainer

    private float originalWidth;   // Stores the original width of the mask

    void Start()
    {
        if (maskRect == null)
        {
            Debug.LogError("Mask RectTransform is not assigned!");
            return;
        }

        // Store the original width of the mask at the start
        originalWidth = maskRect.sizeDelta.x;

        // Ensure the slider updates the mask when it changes
        slider.onValueChanged.AddListener(UpdateMask);
    }

    void UpdateMask(float value)
    {
        // Adjust the width of the mask based on the slider's normalized value
        maskRect.sizeDelta = new Vector2(originalWidth * value, maskRect.sizeDelta.y);
    }
}
