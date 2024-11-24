using UnityEngine;

public class ShellBoxManager : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager; // Reference to the UIManager script
    public GameObject[] shellBoxes; // Array of all Shell Box GameObjects

    void Start()
    {
        // Ensure the UIManager is assigned
        if (uiManager == null)
        {
            Debug.LogError("UIManager is not assigned in the Inspector.");
            return;
        }

        // Ensure the shellBoxes array is populated
        if (shellBoxes == null || shellBoxes.Length == 0)
        {
            Debug.LogError("ShellBoxes array is empty. Please assign Shell Box GameObjects in the Inspector.");
            return;
        }

        UpdateShellBoxes();
    }

    void Update()
    {
        UpdateShellBoxes();
    }

    private void UpdateShellBoxes()
    {
        // Get the current value of Ammo Packs from the UIManager
        int ammoPacks = Mathf.Clamp((int)uiManager.ammoPacks, 0, shellBoxes.Length);

        // Enable or disable shell boxes based on the ammoPacks value
        for (int i = 0; i < shellBoxes.Length; i++)
        {
            if (shellBoxes[i] == null)
            {
                continue; // Skip this iteration if the shell box is null
            }

            // Enable or disable the shell box
            if (i < ammoPacks)
            {
                if (!shellBoxes[i].activeSelf)
                {
                    shellBoxes[i].SetActive(true);
                    Debug.Log($"Enabled: {shellBoxes[i].name}");
                }
            }
            else
            {
                if (shellBoxes[i].activeSelf)
                {
                    // Disable the shell box but preserve shell count
                    ShellBoxSpawner spawner = shellBoxes[i].GetComponent<ShellBoxSpawner>();
                    if (spawner != null)
                    {
                        spawner.enabled = false; // Disable the spawner logic
                    }
                    shellBoxes[i].SetActive(false);
                    Debug.Log($"Disabled: {shellBoxes[i].name}");
                }
            }
        }
    }
}
