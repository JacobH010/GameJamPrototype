using UnityEngine;
using TMPro;

public class MainMenuManager1 : MonoBehaviour
{
    public TextMeshProUGUI highScoreText; // Reference this in the inspector
    private int displayedHighScore = -1; // Tracks the currently displayed high score

    private void Update()
    {
        if (ScoreManager.scoreManager != null)
        {
            int currentHighScore = ScoreManager.scoreManager.ReadHighScore();
            Debug.Log($"Checking high score. Current: {currentHighScore}, Displayed: {displayedHighScore}");

            // Update the text only if the high score has changed
            if (currentHighScore != displayedHighScore)
            {
                displayedHighScore = currentHighScore;
                highScoreText.text = "High Score: $" + currentHighScore.ToString();
                Debug.Log($"High score updated to: {currentHighScore}");
            }
        }
        else
        {
            Debug.LogError("ScoreManager instance is missing! Unable to check or update high score.");
        }
    }
}
