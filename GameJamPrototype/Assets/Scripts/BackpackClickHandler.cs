using System.Collections;
using UnityEngine;

public class BackpackUIClickHandler : MonoBehaviour
{
    // References to Backpack and OpenBackpack GameObjects
    public GameObject backpack;
    public GameObject openBackpack;

    // Assign the target UI element to scale in the Inspector
    public RectTransform targetUIElement;

    // Duration of the scaling animation
    public float scaleDuration = 0.5f;

    // Target scale increment for opening
    public Vector3 scaleIncrement = Vector3.one;

    // Initial scale of the target
    private Vector3 initialScale;

    private void Start()
    {
        // Store the initial scale of the target UI element
        if (targetUIElement != null)
        {
            initialScale = targetUIElement.localScale;
        }
    }

    // Method to open the backpack
    public void OnBackpackClick()
    {
        if (backpack != null && targetUIElement != null)
        {
            Debug.Log($"Backpack clicked. Starting smooth scale for {targetUIElement.name}");

            // Start the scaling animation
            StartCoroutine(SmoothScale(targetUIElement, targetUIElement.localScale + scaleIncrement, scaleDuration));

            // Disable the Backpack GameObject
            backpack.SetActive(false);
            Debug.Log($"{backpack.name} has been disabled.");

            // Enable the OpenBackpack GameObject
            if (openBackpack != null)
            {
                openBackpack.SetActive(true);
                Debug.Log($"{openBackpack.name} has been enabled.");
            }
        }
        else
        {
            Debug.LogWarning("Backpack or Target UI Element is not assigned. Please assign them in the Inspector.");
        }
    }

    // Method to close the backpack using the X button
    public void OnCloseBackpackClick()
    {
        if (openBackpack != null && targetUIElement != null)
        {
            Debug.Log($"Close button clicked. Reverting scale for {targetUIElement.name}");

            // Start the scaling animation in reverse
            StartCoroutine(SmoothScale(targetUIElement, initialScale, scaleDuration, () =>
            {
                // Disable the OpenBackpack GameObject after scaling down
                openBackpack.SetActive(false);
                Debug.Log($"{openBackpack.name} has been disabled.");

                // Enable the Backpack GameObject
                if (backpack != null)
                {
                    backpack.SetActive(true);
                    Debug.Log($"{backpack.name} has been enabled.");
                }
            }));
        }
        else
        {
            Debug.LogWarning("OpenBackpack or Target UI Element is not assigned. Please assign them in the Inspector.");
        }
    }

    private IEnumerator SmoothScale(RectTransform target, Vector3 targetScale, float duration, System.Action onComplete = null)
    {
        Vector3 startScale = target.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            // Use Mathf.SmoothStep to interpolate with an ease-out effect
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);

            // Smoothly interpolate the scale
            target.localScale = Vector3.Lerp(startScale, targetScale, easedProgress);

            yield return null; // Wait for the next frame
        }

        // Ensure the final scale is set exactly
        target.localScale = targetScale;
        Debug.Log($"Interpolated smooth scale completed for {target.name}");

        // Call the onComplete action if provided
        onComplete?.Invoke();
    }
}
