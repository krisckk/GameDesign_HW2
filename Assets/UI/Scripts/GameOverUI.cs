
using UnityEngine;
using UnityEngine.InputSystem;

public class GameOverUI : MonoBehaviour
{
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject pausePanel;
    public AudioClip winClip;

    private bool isGameOver = false;
    private bool isPaused = false;

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

        if (winPanel != null)
            winPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.pKey.wasPressedThisFrame)
            return;

        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (isGameOver || isPaused || pausePanel == null) return;
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.IsTransitioning) return;

        isPaused = true;
        pausePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;
        pausePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
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

    public bool ShowWin()
    {
        if (winPanel == null) return false;

        isGameOver = true;
        winPanel.SetActive(true);
        PlayWinMusic();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        return true;
    }

    void PlayWinMusic()
    {
        if (winClip == null) return;

        // 停掉關卡背景音樂與其他聲音，改播勝利音樂
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
            source.Stop();

        AudioSource winSource = gameObject.AddComponent<AudioSource>();
        winSource.clip = winClip;
        winSource.Play();
    }

    public void RestartGame()
    {
        Debug.Log("Restart Button Clicked!");
        GameFlowManager.Instance.RestartGame();
    }

    public void BackToMenu()
    {
        Debug.Log("Main Menu Button Clicked!");
        GameFlowManager.Instance.LoadMainMenu();
    }
}
