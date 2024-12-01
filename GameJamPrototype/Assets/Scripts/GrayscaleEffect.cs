using UnityEngine;

public class GrayscaleEffect : MonoBehaviour
{
    public Material grayscaleMaterial; // Assign the material with the grayscale shader.
    [Range(0, 1)] public float intensity = 0; // Grayscale intensity controlled by health.

    private void Awake()
    {
        // Initialize grayscale intensity based on default health value (e.g., 100).
        UpdateGrayscaleIntensity(100f);
    }

    // Call this method when the health slider value changes.
    public void UpdateGrayscaleIntensity(float health)
    {
        // Assuming the health slider's max value is 100 and min value is 0.
        intensity = Mathf.Clamp01(1 - (health / 100f));
        grayscaleMaterial.SetFloat("_Intensity", intensity);
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (grayscaleMaterial != null)
        {
            Graphics.Blit(src, dest, grayscaleMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
