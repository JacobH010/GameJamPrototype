using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameComplete : MonoBehaviour
{
    public TMP_Text gameCompleteText;
    public Button yesButton;
    public Button noButton;
    public Button sceneSwitchButton; // Button now used for resetting the game
    public GameObject blackImage;
    public TMP_Text scoreText;
    public ScoreManager scoreManager;
    public string targetSceneName; // Specify the initial scene name for the reset

    private bool hasEnteredBefore = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger zone.");
            if (hasEnteredBefore)
            {
                ActivateGameComplete(other.gameObject);
                Debug.Log("Player re-entered the trigger zone. GameComplete UI activated.");
            }
            else
            {
                hasEnteredBefore = true;
                Debug.Log("First time entering the trigger zone. Setting hasEnteredBefore to true.");
            }
        }
    }

    private void ActivateGameComplete(GameObject player)
    {
        Time.timeScale = 0;
        Debug.Log("Time scale set to 0. Game paused.");

        if (gameCompleteText != null)
        {
            gameCompleteText.gameObject.SetActive(true);
            gameCompleteText.text = "Are you ready to leave?";
            Debug.Log("GameComplete text activated and updated.");
        }

        if (yesButton != null && noButton != null)
        {
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);

            yesButton.onClick.AddListener(() => OnYesButtonClicked(player));
            noButton.onClick.AddListener(OnNoButtonClicked);

            Debug.Log("Yes and No buttons activated and listeners added.");
        }
    }

    private void OnYesButtonClicked(GameObject player)
    {
        Debug.Log("Yes button clicked.");

        if (blackImage != null)
        {
            blackImage.SetActive(true);
            Debug.Log("Black image activated.");
        }

        if (scoreManager != null && scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = $"Total Value: {scoreManager.currentScore}";
            scoreManager.SaveScore();
            Debug.Log($"Score displayed: {scoreManager.currentScore}. Score saved.");
        }

        HideUI();

        // Enable the reset button
        if (sceneSwitchButton != null)
        {
            sceneSwitchButton.gameObject.SetActive(true);
            sceneSwitchButton.onClick.AddListener(LoadMainMenu);
            Debug.Log("Scene switch (main menu) button activated and listener added.");
        }
    }

    private void OnNoButtonClicked()
    {
        HideUI();
        Time.timeScale = 1;
        Debug.Log("No button clicked. UI hidden and time scale reset to 1.");
    }

    private void LoadMainMenu()
    {
        Debug.Log("LoadMainMenu method called.");

        // Unpause the game
        Time.timeScale = 1;
        Debug.Log("Time scale reset to 1 before switching to the main menu.");

        // Find and destroy the LoadoutManager object
        var loadoutManager = GameObject.Find("LoadoutManager");
        if (loadoutManager != null)
        {
            Destroy(loadoutManager);
            Debug.Log("LoadoutManager destroyed.");
        }
        else
        {
            Debug.LogWarning("LoadoutManager not found.");
        }

        // Find and destroy the AIManager object
        var aiManager = GameObject.Find("AIManager");
        if (aiManager != null)
        {
            Destroy(aiManager);
            Debug.Log("AIManager destroyed.");
        }
        else
        {
            Debug.LogWarning("AIManager not found.");
        }

        // Load the main menu scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Debug.Log($"Loading main menu scene: {targetSceneName}");
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name for the main menu is not specified.");
        }
    }

    private void HideUI()
    {
        Debug.Log("Hiding UI.");

        if (gameCompleteText != null)
        {
            gameCompleteText.gameObject.SetActive(false);
            Debug.Log("GameComplete text hidden.");
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(false);
            yesButton.onClick.RemoveAllListeners();
            Debug.Log("Yes button hidden and listeners removed.");
        }

        if (noButton != null)
        {
            noButton.gameObject.SetActive(false);
            noButton.onClick.RemoveAllListeners();
            Debug.Log("No button hidden and listeners removed.");
        }

        if (sceneSwitchButton != null)
        {
            sceneSwitchButton.gameObject.SetActive(false);
            sceneSwitchButton.onClick.RemoveAllListeners();
            Debug.Log("Scene switch button hidden and listeners removed.");
        }
    }
}
