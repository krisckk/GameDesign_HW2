
using UnityEngine;
using TMPro;

public class CountdownTimer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timerText;

    [Header("Timer Settings")]
    public float totalTime = 60f;

    [Header("Game Over")]
    public GameOverUI gameOverUI;

    private float remainingTime;
    private bool timeUpTriggered = false;
    private bool isStopped = false;

    void Start()
    {
        remainingTime = totalTime;
        timeUpTriggered = false;
        isStopped = false;

        UpdateTimerUI();
    }

    void Update()
    {
        if (isStopped || timeUpTriggered)
            return;

        if (remainingTime > 0f)
        {
            remainingTime -= Time.deltaTime;
            remainingTime = Mathf.Max(0f, remainingTime);

            UpdateTimerUI();
        }

        if (remainingTime <= 0f && !timeUpTriggered)
        {
            TimeUp();
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text =
                Mathf.CeilToInt(remainingTime).ToString();
        }
    }

    void TimeUp()
    {
        timeUpTriggered = true;
        isStopped = true;

        remainingTime = 0f;
        UpdateTimerUI();

        Debug.Log("Time's Up! Game Over!");

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("GameOverUI is not assigned!");
        }
    }

    // 給 GameManager 呼叫，例如過關時停止計時
    public void StopTimer()
    {
        isStopped = true;
    }

    // 重新開始倒數
    public void ResetTimer()
    {
        remainingTime = totalTime;
        timeUpTriggered = false;
        isStopped = false;

        UpdateTimerUI();
    }

    // 取得剩餘時間
    public float GetRemainingTime()
    {
        return remainingTime;
    }
}
