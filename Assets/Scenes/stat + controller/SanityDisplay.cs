using UnityEngine;
using TMPro; // ต้องมีสำหรับ TextMeshPro

public class SanityDisplay : MonoBehaviour
{
    // ⭐ ตัวแปรที่ต้องลากใส่ใน Inspector
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
        // 1. ตรวจสอบการเชื่อมต่อ
        if (playerStats == null || sanityText == null)
        {
            if (playerStats == null) Debug.LogError("Player Stats reference is missing in SanityDisplay.");
            if (sanityText == null) Debug.LogError("Sanity Text UI reference is missing in SanityDisplay.");
            enabled = false;
            return;
        }

        // ⭐ 2. บันทึกตำแหน่งเริ่มต้นของ Text UI (เช่น 20, 20)
        // ตำแหน่งนี้คือตำแหน่งที่ถูกคำนวณจาก Anchor มุมล่างซ้ายแล้ว
        originalTextPosition = sanityText.transform.localPosition;

        // 3. สมัครรับ Event เมื่อ Sanity เปลี่ยน
        playerStats.OnSanityUpdate += UpdateSanityDisplay;

        // 4. ตั้งค่าเริ่มต้น
        UpdateSanityDisplay(playerStats.CurrentSanity, playerStats.maxSanity);
    }

    // ⭐ เมธอดสำหรับ Update การสั่นทุกเฟรม
    void Update()
    {
        if (isShaking && sanityText != null)
        {
            // ใช้ Perlin Noise เพื่อสร้างการสั่นแบบสุ่มที่เป็นระเบียบ
            float x = (Mathf.PerlinNoise(Time.time * shakeSpeed, 0f) * 2f - 1f) * currentShakeMagnitude;
            float y = (Mathf.PerlinNoise(0f, Time.time * shakeSpeed) * 2f - 1f) * currentShakeMagnitude;

            // ⭐ ใช้ originalTextPosition เป็นจุดศูนย์กลางของการสั่น
            // Text จะสั่นรอบๆ ตำแหน่ง (20, 20) ที่เราตั้งไว้แทนที่จะเป็น (0, 0)
            sanityText.transform.localPosition = originalTextPosition + new Vector3(x, y, 0);
        }
    }

    private void UpdateSanityDisplay(float currentSanity, float maxSanity)
    {
        // 1. คำนวณและอัปเดตข้อความเปอร์เซ็นต์
        int percentageInt = Mathf.RoundToInt(currentSanity / maxSanity * 100f);
        sanityText.text = $"Sanity: {percentageInt}%";

        // 2. อัปเดตความรุนแรงของการสั่นตามระดับ Sanity
        if (currentSanity >= mildThreshold)
        {
            currentShakeMagnitude = 0f;
            isShaking = false;
            // ⭐ รีเซ็ตตำแหน่งกลับไปที่ originalTextPosition (เช่น 20, 20) เสมอ
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
        // เลิกสมัครรับ Event เมื่อ GameObject ถูกทำลาย
        if (playerStats != null)
        {
            playerStats.OnSanityUpdate -= UpdateSanityDisplay;
        }
    }
}