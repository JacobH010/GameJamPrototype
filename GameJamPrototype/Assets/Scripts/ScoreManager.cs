using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager scoreManager { get; private set; }
    public int currentScore;
    public int highScore;
    public event System.Action<int> OnHighScoreUpdated;

    [Header("Object References")]
    public TextMeshProUGUI globalScoreText;

    private void Awake()
    {
        if (scoreManager == null)
        {
            scoreManager = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadScore();
        ReconnectScoreDisplay();
        UpdateGlobalScoreDisplay();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}. Attempting to reconnect Score Display.");
        ReconnectScoreDisplay();
        UpdateGlobalScoreDisplay();
    }

    private void ReconnectScoreDisplay()
    {
        if (globalScoreText == null)
        {
            GameObject scoreDisplayObject = GameObject.Find("Score Display");
            if (scoreDisplayObject != null)
            {
                globalScoreText = scoreDisplayObject.GetComponent<TextMeshProUGUI>();
                Debug.Log("Successfully reconnected ScoreManager to the Score Display.");
            }
            else
            {
                Debug.LogWarning("Score Display object not found in the current scene.");
            }
        }
    }

    public void UpdateScore(int score)
    {
        currentScore += score;
        Debug.Log($"Updated current score: {currentScore}");
        CheckHighScore();
        SaveScore();
        UpdateGlobalScoreDisplay();
    }

    private void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            Debug.Log($"New high score: {highScore}");
            OnHighScoreUpdated?.Invoke(highScore);
        }
    }

    private void UpdateGlobalScoreDisplay()
    {
        if (globalScoreText != null)
        {
            globalScoreText.text = "$" + currentScore.ToString();
            Debug.Log($"Global score display updated to: {currentScore}");
        }
        else
        {
            Debug.LogWarning("Global score text is not assigned.");
        }
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("CurrentScore", currentScore);
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
        Debug.Log($"Scores saved. Current Score: {currentScore}, High Score: {highScore}");
    }

    public void LoadScore()
    {
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        Debug.Log($"Scores loaded. Current Score: {currentScore}, High Score: {highScore}");
    }

    public int ReadHighScore()
    {
        return highScore;
    }
}
