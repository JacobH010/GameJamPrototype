using UnityEngine;
using UnityEngine.UI;

public class EnableAndDeleteShellBox : MonoBehaviour
{
    [Header("References")]
    public GameObject shellBoxObject; // GameObject containing the ShellBoxSpawner component
    public GameObject xButton;        // The X Button GameObject to enable
    public UIManager uiManager;       // Reference to the UIManager script

    private ShellBoxSpawner shellBoxSpawner; // Reference to the ShellBoxSpawner component

    void Start()
    {
        Debug.Log("EnableAndDeleteShellBox script initialized.");

        if (shellBoxObject == null)
        {
            Debug.LogError("ShellBoxObject is not assigned in the Inspector.");
            return;
        }
        Debug.Log($"ShellBoxObject assigned: {shellBoxObject.name}");

        shellBoxSpawner = shellBoxObject.GetComponent<ShellBoxSpawner>();
        if (shellBoxSpawner == null)
        {
            return;
        }
        Debug.Log("ShellBoxSpawner component found.");

        if (xButton == null)
        {
            Debug.LogError("X Button is not assigned in the Inspector.");
            return;
        }
        Debug.Log($"X Button assigned: {xButton.name}");

        if (uiManager == null)
        {
            Debug.LogError("UIManager is not assigned in the Inspector.");
            return;
        }
        Debug.Log("UIManager reference assigned.");

        // Initially set the X Button to inactive
        xButton.SetActive(false);
        Debug.Log("X Button set to inactive initially.");
    }

    void Update()
    {
        if (shellBoxSpawner == null || xButton == null) return;

        if (shellBoxSpawner.shellCount <= 0 && !xButton.activeSelf)
        {
            // Enable the X Button
            xButton.SetActive(true);
            Debug.Log($"X Button enabled because shell count of {shellBoxObject.name} is zero.");
        }
        else if (shellBoxSpawner.shellCount > 0 && xButton.activeSelf)
        {
            // Disable the X Button if the shell count is no longer zero
            xButton.SetActive(false);
            Debug.Log($"X Button disabled because shell count of {shellBoxObject.name} is {shellBoxSpawner.shellCount}.");
        }
    }

    // Function to delete the parent GameObject and reduce ammo packs
    public void DeleteParent()
    {
        Debug.Log("DeleteParent method triggered.");

        if (shellBoxObject != null)
        {
            Debug.Log($"Deleting shell box object: {shellBoxObject.name}");
            Destroy(shellBoxObject);

            // Reduce ammo packs in UIManager
            if (uiManager != null)
            {
                uiManager.ammoPacks = Mathf.Max(0, uiManager.ammoPacks - 1);
                Debug.Log($"Ammo Packs reduced by 1. New value: {uiManager.ammoPacks}");
            }
            else
            {
                Debug.LogWarning("UIManager reference is null. Cannot reduce Ammo Packs.");
            }
        }
        else
        {
            Debug.LogWarning("ShellBoxObject is already null. Cannot delete.");
        }
    }
}
