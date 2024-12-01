using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResetManager : MonoBehaviour
{
    [SerializeField] private string initialSceneName = "MainMenu"; // Set this in the Inspector

    // This method can be linked to a UI button
    public void OnResetButtonPressed()
    {
        ResetGame();
    }

    private void ResetGame()
    {
        // Clear PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // Destroy all persistent objects
        CleanupDontDestroyOnLoad();

        // Reload the initial scene
        SceneManager.LoadScene(initialSceneName);
    }

    private void CleanupDontDestroyOnLoad()
    {
        // Find all root objects in the DontDestroyOnLoad scene
        var dontDestroyOnLoadScene = SceneManager.GetSceneByName("DontDestroyOnLoad");
        if (dontDestroyOnLoadScene.IsValid())
        {
            foreach (var rootObject in dontDestroyOnLoadScene.GetRootGameObjects())
            {
                Destroy(rootObject);
            }
        }
    }
}
