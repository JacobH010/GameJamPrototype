using UnityEngine;
using UnityEngine.UI;

public class O2TankState : MonoBehaviour
{
    [SerializeField, Tooltip("Indicates if the tank is flagged as active")]
    private bool isActive = false;

    public bool IsActive
    {
        get => isActive;
        set
        {
            if (isActive != value) // Only act if the value changes
            {
                isActive = value;
                if (isActive)
                {
                    AssignSliderToUIManager();
                }
            }
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
