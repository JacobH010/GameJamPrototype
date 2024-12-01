using UnityEngine;
using TMPro;

public class MainMenuManager1 : MonoBehaviour
{
    public TextMeshProUGUI highScoreText; // Reference this in the inspector

    private void Start()
    {
        if (ScoreManager.scoreManager != null)
        {
            int highScore = ScoreManager.scoreManager.ReadHighScore();
            highScoreText.text = "High Score: $" + highScore.ToString();
        }
        else
        {
            Debug.LogError("ScoreManager instance is missing!");
        }
    }
}
