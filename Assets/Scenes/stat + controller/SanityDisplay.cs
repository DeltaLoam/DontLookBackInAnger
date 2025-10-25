using UnityEngine;
using TMPro;

public class SanityDisplay : MonoBehaviour
{
    // ⭐ UI References
    [Header("UI References")]
    public PlayerStats playerStats;
    public TextMeshProUGUI sanityText;

    // ⭐ Text Shake Settings
    [Header("Text Shake Settings")]
    public float mildShakeMagnitude = 2.0f;
    public float mediumShakeMagnitude = 4.0f;
    public float severeShakeMagnitude = 7.0f;
    public float shakeSpeed = 50f;

    // ตัวแปร Thresholds (ควรตรงกับ SanityVisual)
    public float mildThreshold = 75f;
    public float mediumThreshold = 40f;
    public float severeThreshold = 15f;

    private float currentShakeMagnitude = 0f;
    private bool isShaking = false;
    private Vector3 originalTextPosition; // ⭐ ตัวแปรสำคัญสำหรับ Fix ตำแหน่ง

    void Start()
    {
        if (playerStats == null || sanityText == null)
        {
            if (playerStats == null) Debug.LogError("Player Stats reference is missing in SanityDisplay.");
            if (sanityText == null) Debug.LogError("Sanity Text UI reference is missing in SanityDisplay.");
            enabled = false;
            return;
        }

        // ⭐ บันทึกตำแหน่งเริ่มต้นของ Text UI (เช่น 20, 20)
        originalTextPosition = sanityText.transform.localPosition;

        playerStats.OnSanityUpdate += UpdateSanityDisplay;

        UpdateSanityDisplay(playerStats.CurrentSanity, playerStats.maxSanity);
    }

    void Update()
    {
        if (isShaking && sanityText != null)
        {
            float x = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f) * currentShakeMagnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f) * currentShakeMagnitude;

            // ⭐ ใช้ originalTextPosition เป็นจุดศูนย์กลางของการสั่น
            sanityText.transform.localPosition = originalTextPosition + new Vector3(x, y, 0);
        }
    }

    private void UpdateSanityDisplay(float currentSanity, float maxSanity)
    {
        // 1. อัปเดตข้อความเปอร์เซ็นต์
        int percentageInt = Mathf.RoundToInt(currentSanity / maxSanity * 100f);
        sanityText.text = $"{percentageInt}%";

        // 2. อัปเดตความรุนแรงของการสั่น
        if (currentSanity >= mildThreshold)
        {
            currentShakeMagnitude = 0f;
            isShaking = false;
            // ⭐ รีเซ็ตตำแหน่งกลับไปที่ตำแหน่งเริ่มต้น (Fix)
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
        else // Severe
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
    }
}