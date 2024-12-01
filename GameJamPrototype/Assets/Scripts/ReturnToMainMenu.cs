using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenu : MonoBehaviour
{
    [Header("Main Menu Settings")]
    [Tooltip("Name of the main menu scene to load.")]
    public string mainMenuSceneName;

    [Header("Game Objects to Destroy")]
    [Tooltip("Name of the LoadoutManager GameObject.")]
    public string loadoutManagerName = "LoadoutManager";

    [Tooltip("Name of the AIManager GameObject.")]
    public string aiManagerName = "AIManager";

    // Method to be assigned to the UIButton
    public void OnButtonPressed()
    {
        // Unpause the game
        Time.timeScale = 1;
        Debug.Log("Game unpaused (Time.timeScale set to 1).");

        // Destroy the LoadoutManager
        DestroyObjectIfExists(loadoutManagerName);

        // Destroy the AIManager
        DestroyObjectIfExists(aiManagerName);

        // Load the main menu scene
        LoadMainMenu();
    }

    // Helper method to find and destroy objects
    private void DestroyObjectIfExists(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Destroy(obj);
            Debug.Log($"{objectName} destroyed.");
        }
        else
        {
            Debug.LogWarning($"{objectName} not found.");
        }
    }

    // Load the main menu scene
    private void LoadMainMenu()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.Log($"Loading main menu scene: {mainMenuSceneName}");
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("Main menu scene name is not set. Please assign it in the Inspector.");
        }
    }
}
