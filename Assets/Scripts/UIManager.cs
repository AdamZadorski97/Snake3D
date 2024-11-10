using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public GameObject gameOverPanel;
    public Button restartButton;
    public Button acceptName;
    public TMP_Text scoreText;             // Current score display
    public TMP_Text topScoresText;         // Top 10 scores display on game over
    public TMP_InputField playerNameInput; // Input field for player's name on game over

    private int currentScore = 0;
    private List<(string, int)> highScores = new List<(string, int)>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        gameOverPanel.SetActive(false);
       // scoreText.text = "Score: 0";
        restartButton.onClick.AddListener(RestartGame);
        acceptName.onClick.AddListener(AddHighScore);
        LoadHighScores();
    }

    public void UpdateScore(int score)
    {
        currentScore = score;
      //  scoreText.text = "Score: " + currentScore;
    }

    public void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        topScoresText.text = GetTopScoresText();
    }

    public void AddHighScore()
    {
        string playerName = playerNameInput.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Player";

        highScores.Add((playerName, currentScore));
        highScores = highScores.OrderByDescending(score => score.Item2).Take(10).ToList();
        SaveHighScores();
        topScoresText.text = GetTopScoresText();
    }

    private string GetTopScoresText()
    {
        return string.Join("\n", highScores.Select((score, index) => $"{index + 1}. {score.Item1}: {score.Item2}"));
    }

    private void SaveHighScores()
    {
        for (int i = 0; i < highScores.Count; i++)
        {
            PlayerPrefs.SetString($"HighScoreName{i}", highScores[i].Item1);
            PlayerPrefs.SetInt($"HighScoreValue{i}", highScores[i].Item2);
        }
        PlayerPrefs.Save();
    }

    private void LoadHighScores()
    {
        highScores.Clear();
        for (int i = 0; i < 10; i++)
        {
            if (PlayerPrefs.HasKey($"HighScoreName{i}") && PlayerPrefs.HasKey($"HighScoreValue{i}"))
            {
                string name = PlayerPrefs.GetString($"HighScoreName{i}");
                int score = PlayerPrefs.GetInt($"HighScoreValue{i}");
                highScores.Add((name, score));
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
