using UnityEngine;
using TMPro;
using System;

public class SanityDisplay : MonoBehaviour
{
    // ⭐ UI References
    [Header("UI References")]
    [Tooltip("PlayerStats จะถูกตั้งค่าโดย Local Player ผ่าน Event")]
    public PlayerStats playerStats;
    public TextMeshProUGUI sanityText;

    // ⭐ Text Shake Settings
    [Header("Text Shake Settings")]
    public float mildShakeMagnitude = 2.0f;
    public float mediumShakeMagnitude = 4.0f;
    public float severeShakeMagnitude = 7.0f;
    public float shakeSpeed = 50f;

    // ตัวแปร Thresholds
    public float mildThreshold = 75f;
    public float mediumThreshold = 40f;
    public float severeThreshold = 15f;

    private float currentShakeMagnitude = 0f;
    private bool isShaking = false;
    private Vector3 originalTextPosition;

    void Awake()
    {
        // ⭐ สมัครรับสัญญาณจาก PlayerStats ทันทีที่ UI เริ่มทำงาน
        PlayerStats.OnLocalPlayerStatsReady += SetPlayerStats;
    }

    void Start()
    {
        if (sanityText == null)
        {
            Debug.LogError("SanityDisplay: Sanity Text UI reference is missing.");
            enabled = false;
            return;
        }

        originalTextPosition = sanityText.transform.localPosition;

        // กรณีฉุกเฉิน: ถ้า PlayerStats ถูกลากใส่ไว้ก่อน
        if (playerStats != null)
        {
            SetupDisplay(playerStats);
        }
    }

    private void SetPlayerStats(PlayerStats stats)
    {
        // ป้องกันการตั้งค่าซ้ำ
        if (playerStats != null)
        {
            playerStats.OnSanityUpdate -= UpdateSanityDisplay;
        }

        // ตั้งค่าและเริ่มระบบ
        playerStats = stats;
        SetupDisplay(playerStats);
    }

    private void SetupDisplay(PlayerStats stats)
    {
        if (stats == null || sanityText == null) return;

        stats.OnSanityUpdate += UpdateSanityDisplay;

        UpdateSanityDisplay(stats.CurrentSanity, stats.maxSanity);
        Debug.Log("SanityDisplay: Successfully attached to Local Player Stats.");

        // ⭐ ยกเลิกการสมัครรับ Event Static เพื่อลด Overhead เมื่อทำงานเสร็จแล้ว
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }


    void Update()
    {
        // Logic การสั่นของ Text
        if (isShaking && sanityText != null)
        {
            float x = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f) * currentShakeMagnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f) * currentShakeMagnitude;

            sanityText.transform.localPosition = originalTextPosition + new Vector3(x, y, 0);
        }
    }

    private void UpdateSanityDisplay(float currentSanity, float maxSanity)
    {
        int percentageInt = Mathf.RoundToInt(currentSanity / maxSanity * 100f);
        sanityText.text = $"Sanity: {percentageInt}%";

        if (currentSanity >= mildThreshold)
        {
            currentShakeMagnitude = 0f;
            isShaking = false;
            sanityText.transform.localPosition = originalTextPosition;
        }
        else if (currentSanity >= mediumThreshold)
        {
            currentShakeMagnitude = mildShakeMagnitude;
            isShaking = true;
        }
        else if (currentSanity >= severeThreshold)
        {
            currentShakeMagnitude = mediumShakeMagnitude;
            isShaking = true;
        }
        else
        {
            currentShakeMagnitude = severeShakeMagnitude;
            isShaking = true;
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnSanityUpdate -= UpdateSanityDisplay;
        }
        // ⭐ ยกเลิกการสมัครรับ Event Static
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }
}