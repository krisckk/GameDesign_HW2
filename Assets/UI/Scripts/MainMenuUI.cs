
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public Button loadButton;

    void Start()
    {
        // 沒有存檔時停用讀取按鈕
        if (loadButton != null)
            loadButton.interactable = GameFlowManager.HasSave();
    }

    public void StartGame()
    {
        GameFlowManager.Instance.StartGame();
    }

    public void LoadGame()
    {
        GameFlowManager.Instance.LoadGame();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
