using UnityEngine;

public class EnableXButtonOnEmptyShellBox : MonoBehaviour
{
    [Header("References")]
    public GameObject shellBoxObject; // GameObject containing the ShellBoxSpawner component
    public GameObject xButton;       // The X Button GameObject to enable

    private ShellBoxSpawner shellBoxSpawner; // Reference to the ShellBoxSpawner component

    void Start()
    {
        // Ensure the shellBoxObject is assigned
        if (shellBoxObject == null)
        {
            Debug.LogError("ShellBoxObject is not assigned in the Inspector.");
            return;
        }

        // Get the ShellBoxSpawner component from the assigned GameObject
        shellBoxSpawner = shellBoxObject.GetComponent<ShellBoxSpawner>();

        if (shellBoxSpawner == null)
        {
            Debug.LogError($"ShellBoxSpawner component not found on {shellBoxObject.name}.");
            return;
        }

        // Ensure the X Button is assigned
        if (xButton == null)
        {
            Debug.LogError("X Button is not assigned in the Inspector.");
            return;
        }

        // Initially set the X Button to inactive
        xButton.SetActive(false);
    }

    void Update()
    {
        // Check if the shell count is zero
        if (shellBoxSpawner.shellCount <= 0 && !xButton.activeSelf)
        {
            // Enable the X Button
            xButton.SetActive(true);
            Debug.Log($"X Button enabled because shell count of {shellBoxObject.name} is zero.");
        }
    }
}
