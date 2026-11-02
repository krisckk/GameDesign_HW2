
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider hpSlider;
    public TMP_Text hpText;

    void Start()
    {
        UpdateHealthUI();
    }

    void Update()
    {
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (PlayerStats.Instance == null)
            return;

        int currentHP = PlayerStats.Instance.currentHP;
        int maxHP = PlayerStats.Instance.maxHP;

        // 更新血量條
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }

        // 更新血量數字
        if (hpText != null)
        {
            hpText.text = currentHP.ToString();
        }
    }
}
