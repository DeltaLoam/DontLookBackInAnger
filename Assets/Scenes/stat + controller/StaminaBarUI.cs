using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class StaminaBarUI : MonoBehaviour
{
    [Header("UI References")]
    public Slider staminaSlider;
    public TextMeshProUGUI staminaText;
    public PlayerStats playerStats;

    [Header("Exhaustion Visuals")]
    public Color exhaustedColor = Color.red;
    public Color normalColor = Color.green;
    private Image fillImage;

    void Awake()
    {
        // ⭐ ยังคงสมัครรับ Event เผื่อไว้สำหรับการเชื่อมต่ออื่นๆ
        PlayerStats.OnLocalPlayerStatsReady += SetPlayerStats;

        if (staminaSlider != null && staminaSlider.fillRect != null)
        {
            fillImage = staminaSlider.fillRect.GetComponent<Image>();
        }
    }

    // ⭐⭐⭐ แก้ไข Error CS0103: เปลี่ยนจาก private เป็น public ⭐⭐⭐
    public void SetPlayerStats(PlayerStats stats)
    {
        // ป้องกันการตั้งค่าซ้ำ
        if (playerStats != null)
        {
            playerStats.OnStaminaUpdate -= UpdateStaminaBar;
            playerStats.OnExhausted -= OnExhausted;
            playerStats.OnNotExhausted -= OnNotExhausted;
        }

        // ตั้งค่าและเริ่มระบบ
        playerStats = stats;
        SetupDisplay(playerStats);
    }

    private void SetupDisplay(PlayerStats stats)
    {
        if (stats == null || staminaSlider == null) return;

        stats.OnStaminaUpdate += UpdateStaminaBar;
        stats.OnExhausted += OnExhausted;
        stats.OnNotExhausted += OnNotExhausted;

        UpdateStaminaBar(stats.CurrentStaminaReadOnly, stats.maxStamina);

        // เรายกเลิกการสมัครรับ Event ใน SetPlayerStats/SetTarget
    }

    // ⭐⭐⭐ เมธอดที่ UIAssigner.cs ใช้เรียก (แก้ Error CS1061 ใน UIAssigner) ⭐⭐⭐
    public void SetTarget(PlayerStats stats)
    {
        // ยกเลิกการสมัครรับ Event Static เพื่อป้องกันการเชื่อมต่อซ้ำซ้อน
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;

        // เรียกใช้ Logic การตั้งค่า PlayerStats ที่เราเพิ่งเปลี่ยนเป็น public
        SetPlayerStats(stats);
    }

    private void UpdateStaminaBar(float currentStamina, float maxStamina)
    {
        if (staminaSlider == null) return;

        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;

        if (staminaText != null)
        {
            int percentage = Mathf.RoundToInt(currentStamina / maxStamina * 100f);
            staminaText.text = $"Stamina: {percentage}%";
        }

        if (fillImage != null && playerStats != null && !playerStats.IsExhausted)
        {
            fillImage.color = normalColor;
        }
    }

    private void OnExhausted()
    {
        if (fillImage != null)
        {
            fillImage.color = exhaustedColor;
        }
    }

    private void OnNotExhausted()
    {
        if (fillImage != null)
        {
            fillImage.color = normalColor;
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnStaminaUpdate -= UpdateStaminaBar;
            playerStats.OnExhausted -= OnExhausted;
            playerStats.OnNotExhausted -= OnNotExhausted;
        }

        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }
}