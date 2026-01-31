using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI highScoreText;
    public GameObject inGameUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Setup(int score,int highScore)
    {
        gameObject.SetActive(true);
        pointsText.text = "Score: " + score.ToString();
        highScoreText.text = "Best: " + highScore.ToString();

        if (inGameUI != null)
        {
            inGameUI.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Snake");
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
