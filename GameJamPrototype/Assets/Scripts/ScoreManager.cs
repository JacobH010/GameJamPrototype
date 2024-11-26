using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager scoreManager { get; private set; }
    private int currentScore;
    private int score;
    private int highScore;
    public int savedHighScore { get; private set; }

    [Header("Object References")]
    public TextMeshProUGUI globalScoreText;

    private void Awake()
    {
        if (scoreManager == null)
        {
            {
                scoreManager = this;
                DontDestroyOnLoad(gameObject); //prevents the manager from being destroyed when a level is loaded

                //Find the player in teh scene by tag
            }
            //ABSTRACTION
            ResetHighScore();
            
        }
        else
        {
            Destroy(gameObject);//destroys duplicate Score Managers
        }
        LoadHighScore();
        currentScore = 0;
    }
    public void UpdateScore(int score)
    {
        if (globalScoreText != null)
        {
            currentScore += score;
            globalScoreText.text = "Total Data Value: $" + currentScore.ToString();
        }else if (globalScoreText == null)
        {
            Debug.LogError("globalScoreText in Score Manager is null. Object reference missing");
        }
    }
    public void SaveScore()
    {
        score = currentScore;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore); // Save high score persistently
            PlayerPrefs.Save(); // Ensure data is written to storage
        }
    }
    public void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0); // Default to 0 if no high score is saved
        savedHighScore = highScore;

    }

    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.DeleteKey("HighScore");
    }

    public int ReadHighScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        return highScore;
    }
}
