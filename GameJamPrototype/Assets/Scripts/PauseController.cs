using UnityEngine;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu;          // Assign the Pause Menu Canvas
    [SerializeField] private GameObject settingsBackground; // Assign the SettingsBackground GameObject
    [SerializeField] private GameObject clickManager;       // Assign the ClickManager GameObject
    [SerializeField] private GameObject uiFinalized;        // Assign the UIFinalized GameObject
    private bool isPaused = false;                          // Tracks if the game is paused

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Freeze time
        isPaused = true;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true); // Show the pause menu
        }

        if (clickManager != null)
        {
            clickManager.SetActive(false); // Disable the ClickManager
        }

        if (uiFinalized != null)
        {
            // Disable the GraphicRaycaster component of UIFinalized
            GraphicRaycaster raycaster = uiFinalized.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = false;
            }
        }

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume time
        isPaused = false;

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false); // Hide the pause menu
        }

        if (settingsBackground != null)
        {
            settingsBackground.SetActive(false); // Disable the settings background
        }

        if (clickManager != null)
        {
            clickManager.SetActive(true); // Re-enable the ClickManager
        }

        if (uiFinalized != null)
        {
            // Re-enable the GraphicRaycaster component of UIFinalized
            GraphicRaycaster raycaster = uiFinalized.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.enabled = true;
            }
        }

        Debug.Log("Game Resumed");
    }

    // Quit the game
    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
