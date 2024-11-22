using UnityEngine;
using UnityEngine.UI;

public class SliderMaskController : MonoBehaviour
{
    public Slider o2Slider; // Reference to the slider
    public RectTransform maskRect; // Reference to the mask's RectTransform
    public float maxHeight; // Full height of the mask when O2 is full

    void Start()
    {
        // Initialize maxHeight to the starting height of the mask
        if (maskRect != null)
            maxHeight = maskRect.sizeDelta.y;
    }

    void Update()
    {
        if (o2Slider != null && maskRect != null)
        {
            // Adjust the mask height based on the slider value
            float height = maxHeight * (o2Slider.value / o2Slider.maxValue);
            maskRect.sizeDelta = new Vector2(maskRect.sizeDelta.x, height);
        }
    }
}