using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 遊戲開始時自動建立並跨場景保留，不需要放進任何 Scene
public class GameFlowManager : MonoBehaviour
{
    const string MainMenuScene = "MainMenu";
    const string SaveKey = "HW2_Save";
    static readonly string[] Levels = { "Level1", "Level2", "Level3" };

    [Serializable]
    class SaveData
    {
        public int level;
        public int hp;
        public int score;
        public float elapsedTime;
    }

    public static GameFlowManager Instance { get; private set; }

    public float ElapsedTime { get; private set; }

    public bool IsTransitioning => transitioning;

    public float clearDelay = 2.5f;     // 最後一隻敵人死亡後，等死亡動畫播完再換關；三關統一
    public float fadeDuration = 0.5f;

    CanvasGroup fade;
    int currentLevel = -1;
    bool levelEnding;
    bool transitioning;
    bool playerDead;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance == null)
            new GameObject("GameFlowManager").AddComponent<GameFlowManager>();
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        CreateFade();
        EnterScene(SceneManager.GetActiveScene());
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnemyCountTarget.AnyDied += OnEnemyDied;
        PlayerHealth.Died += OnPlayerDied;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        EnemyCountTarget.AnyDied -= OnEnemyDied;
        PlayerHealth.Died -= OnPlayerDied;
    }

    void Update()
    {
        if (currentLevel >= 0)
            ElapsedTime += Time.deltaTime;
    }

    public void StartGame()
    {
        if (transitioning) return;

        PlayerStats.Instance.ResetAll();
        ElapsedTime = 0f;
        BeginTransition(Levels[0]);
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void LoadMainMenu()
    {
        if (transitioning) return;

        BeginTransition(MainMenuScene);
    }

    public void SaveGame()
    {
        if (currentLevel < 0) return;

        SaveData data = new SaveData
        {
            level = currentLevel,
            hp = PlayerStats.Instance.currentHP,
            score = PlayerStats.Instance.score,
            elapsedTime = ElapsedTime
        };

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SaveKey);
    }

    public void LoadGame()
    {
        if (transitioning || !HasSave()) return;

        SaveData data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));

        PlayerStats.Instance.Restore(data.hp, data.score);
        ElapsedTime = data.elapsedTime;
        BeginTransition(Levels[Mathf.Clamp(data.level, 0, Levels.Length - 1)]);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnterScene(scene);
    }

    void EnterScene(Scene scene)
    {
        currentLevel = Array.IndexOf(Levels, scene.name);
        levelEnding = false;
        playerDead = false;

        // 每次進入關卡時自動存檔
        if (currentLevel >= 0)
            SaveGame();

        if (scene.name == MainMenuScene)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void OnPlayerDied()
    {
        playerDead = true;
    }

    void OnEnemyDied()
    {
        if (currentLevel < 0 || levelEnding || playerDead) return;

        EnemyCountTarget[] enemies =
            FindObjectsByType<EnemyCountTarget>(FindObjectsSortMode.None);

        foreach (EnemyCountTarget enemy in enemies)
        {
            if (enemy.IsAlive) return;
        }

        StartCoroutine(FinishLevel());
    }

    IEnumerator FinishLevel()
    {
        levelEnding = true;

        CountdownTimer timer = FindAnyObjectByType<CountdownTimer>();
        if (timer != null) timer.StopTimer();

        yield return new WaitForSeconds(clearDelay);

        // 等待期間玩家可能被飛行中的魔法彈打死
        if (playerDead) yield break;

        if (currentLevel + 1 < Levels.Length)
        {
            yield return Transition(Levels[currentLevel + 1]);
            yield break;
        }

        // 破關後不留存檔，避免從主選單讀回已通關的進度
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();

        GameOverUI gameOverUI = FindAnyObjectByType<GameOverUI>();
        if (gameOverUI != null && gameOverUI.ShowWin()) yield break;

        Debug.LogWarning("GameOverUI 尚未設定 Win Panel，直接回到主選單");
        yield return Transition(MainMenuScene);
    }

    void BeginTransition(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(Transition(sceneName));
    }

    IEnumerator Transition(string sceneName)
    {
        transitioning = true;

        // 淡出時暫停遊戲，避免黑屏期間玩家還會受傷
        Time.timeScale = 0f;
        yield return Fade(1f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
        yield return null;

        yield return Fade(0f);
        transitioning = false;
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fade.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // 限制單幀時間，避免載入場景的卡頓直接跳過淡入
            elapsed += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            fade.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fade.alpha = targetAlpha;
    }

    void CreateFade()
    {
        GameObject fadeObject = new GameObject("ScreenFade");
        fadeObject.transform.SetParent(transform);

        Canvas canvas = fadeObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        Image image = fadeObject.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        fade = fadeObject.AddComponent<CanvasGroup>();
        fade.alpha = 0f;
        fade.interactable = false;
        fade.blocksRaycasts = false;
    }
}
