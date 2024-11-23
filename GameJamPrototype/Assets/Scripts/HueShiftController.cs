using UnityEngine;
using UnityEngine.UI;

public class HueShiftController : MonoBehaviour
{
    public Material material; // Reference to the material with the custom shader
    public Slider hueSlider;  // Reference to the slider UI element

    void Start()
    {
        hueSlider.onValueChanged.AddListener(OnHueChanged);
    }

    void OnHueChanged(float sliderValue)
    {
        // Map slider value (0 to 100) to hue shift range (0 to 0.311)
        float hueShiftValue = sliderValue / 100f * 0.311f;
        material.SetFloat("_HueShift", hueShiftValue);
    }
}
