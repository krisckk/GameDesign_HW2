using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    static PlayerStats _instance;
    public static PlayerStats Instance
    {
        get
        {
            if (_instance == null) new GameObject("PlayerStats").AddComponent<PlayerStats>();
            return _instance;
        }
    }

    public int maxHP = 100;
    public int currentHP;
    public int score;
    public event Action OnChanged;   // UI subscribes to this

    void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }
        _instance = this;
        DontDestroyOnLoad(gameObject);
        currentHP = maxHP;
    }

    public void ChangeHP(int delta) { currentHP = Mathf.Clamp(currentHP + delta, 0, maxHP); OnChanged?.Invoke(); }
    public void AddScore(int v) { score += v; OnChanged?.Invoke(); }
    public void ResetAll() { currentHP = maxHP; score = 0; OnChanged?.Invoke(); }  // call on New Game / Restart
}