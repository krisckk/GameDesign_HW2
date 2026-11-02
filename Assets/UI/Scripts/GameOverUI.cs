
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;

    private bool isGameOver = false;

    void OnEnable()
    {
        PlayerHealth.Died += ShowGameOver;
    }

    void OnDisable()
    {
        PlayerHealth.Died -= ShowGameOver;
    }

    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    public void ShowGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Debug.Log("Restart Button Clicked!");
        Time.timeScale = 1f;

        // 重新開始時恢復滿血
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.ChangeHP(
                PlayerStats.Instance.maxHP -
                PlayerStats.Instance.currentHP
            );
        }

        // 重新載入第一關
        SceneManager.LoadScene("Level1");
    }

    public void BackToMenu()
    {
        Debug.Log("Main Menu Button Clicked!");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
