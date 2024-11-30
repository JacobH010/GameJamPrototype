using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Import SceneManager namespace
using TMPro;

public class GameComplete : MonoBehaviour
{
    public TMP_Text gameCompleteText;
    public Button yesButton;
    public Button noButton;
    public Button sceneSwitchButton; // Add a new button for switching scenes
    public GameObject blackImage;
    public TMP_Text scoreText;
    public ScoreManager scoreManager;
    public string targetSceneName; // Specify the scene name

    private bool hasEnteredBefore = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (hasEnteredBefore)
            {
                ActivateGameComplete(other.gameObject);
            }
            else
            {
                hasEnteredBefore = true;
            }
        }
    }

    private void ActivateGameComplete(GameObject player)
    {
        Time.timeScale = 0;

        if (gameCompleteText != null)
        {
            gameCompleteText.gameObject.SetActive(true);
            gameCompleteText.text = "Are you ready to leave?";
        }

        if (yesButton != null && noButton != null)
        {
            yesButton.gameObject.SetActive(true);
            noButton.gameObject.SetActive(true);

            yesButton.onClick.AddListener(() => OnYesButtonClicked(player));
            noButton.onClick.AddListener(OnNoButtonClicked);
        }
    }

    private void OnYesButtonClicked(GameObject player)
    {
        if (blackImage != null)
        {
            blackImage.SetActive(true);
        }

        if (scoreManager != null && scoreText != null)
        {
            scoreText.gameObject.SetActive(true);
            scoreText.text = $"Total Value: {scoreManager.currentScore}";
            scoreManager.SaveScore();
        }

        HideUI();

        // Enable the scene switch button
        if (sceneSwitchButton != null)
        {
            sceneSwitchButton.gameObject.SetActive(true);
            sceneSwitchButton.onClick.AddListener(() => SwitchScene());
        }

        Debug.Log("Yes clicked: Scene switch button activated.");
    }

    private void OnNoButtonClicked()
    {
        HideUI();
        Time.timeScale = 1;
        Debug.Log("No clicked: Game resumed without deleting player.");
    }

    private void SwitchScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("Target scene name is not specified.");
        }
    }

    private void HideUI()
    {
        if (gameCompleteText != null)
        {
            gameCompleteText.gameObject.SetActive(false);
        }

        if (yesButton != null)
        {
            yesButton.gameObject.SetActive(false);
            yesButton.onClick.RemoveAllListeners();
        }

        if (noButton != null)
        {
            noButton.gameObject.SetActive(false);
            noButton.onClick.RemoveAllListeners();
        }

        if (sceneSwitchButton != null)
        {
            sceneSwitchButton.gameObject.SetActive(false);
            sceneSwitchButton.onClick.RemoveAllListeners();
        }
    }
}
