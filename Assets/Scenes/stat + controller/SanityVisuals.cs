using UnityEngine;
using UnityEngine.UI;
using System;

// สคริปต์ที่แนบกับกล้อง (Player Camera)
public class SanityVisuals : MonoBehaviour
{
    // การอ้างอิง UI
    [Header("UI Overlay")]
    [Tooltip("ลาก RawImage UI ที่ครอบคลุมหน้าจอมาใส่")]
    public RawImage sanityVignetteImage;
    [Tooltip("ลาก Texture หรือ Sprite Vignette มาใส่")]
    public Texture vignetteTexture;

    private PlayerStats playerStats; // Component สำหรับดึงค่า Sanity

    // พารามิเตอร์ที่ปรับใน Inspector
    [Header("Sanity Effects")]
    // ⭐ เปลี่ยนเป็น 100.0f เพื่อให้ผลกระทบเริ่มทันทีที่ Sanity ลดลงจากค่าสูงสุด
    [Tooltip("Sanity Level ที่ Vignette เริ่มมีผลกระทบ")]
    public float effectStartSanity = 100.0f;

    // ⭐ ปรับให้เร็วขึ้นมากเพื่อให้การ Fade ตอบสนองทันที
    [Tooltip("อัตราความเร็วที่ Vignette จะ Fade (ยิ่งมาก ยิ่งเร็ว)")]
    public float vignetteFadeSpeed = 15.0f;

    // ⭐ ปรับให้โค้งมากขึ้นมาก: เอฟเฟกต์จะรุนแรงเฉพาะตอน Sanity ต่ำมากๆ เท่านั้น
    [Tooltip("ค่ากำลัง (Power) สำหรับกำหนดโค้งการ Fade (แนะนำ 5.0f ขึ้นไป)")]
    public float fadeCurvePower = 5.0f;

    private const float MAX_FADE_ALPHA = 1.0f; // มืดสนิทเมื่อ Sanity = 0

    // ตัวแปรสำหรับควบคุมการ Fade
    private float currentVignetteAlpha = 0f;
    private float targetVignetteAlpha = 0f;

    void Awake()
    {
        playerStats = GetComponentInParent<PlayerStats>();

        if (playerStats == null || sanityVignetteImage == null || vignetteTexture == null)
        {
            Debug.LogError("SanityVisuals setup failed. Check references: PlayerStats, RawImage, and Texture must be assigned.");
            enabled = false;
            return;
        }

        // กำหนด Texture และสีเริ่มต้น
        sanityVignetteImage.texture = vignetteTexture;
        sanityVignetteImage.color = new Color(0f, 0f, 0f, 0f); // สีดำ และ Alpha 0

        playerStats.OnSanityUpdate += UpdateVignetteTarget;

        // ตรวจสอบให้แน่ใจว่า effectStartSanity ไม่เกิน maxSanity ของ PlayerStats
        if (effectStartSanity > playerStats.maxSanity)
        {
            Debug.LogWarning("effectStartSanity was reset to maxSanity to ensure effects start immediately.");
            effectStartSanity = playerStats.maxSanity;
        }
    }

    void Update()
    {
        // การ Fade อย่างนุ่มนวล (Lerp)
        if (currentVignetteAlpha != targetVignetteAlpha)
        {
            currentVignetteAlpha = Mathf.Lerp(
                currentVignetteAlpha,
                targetVignetteAlpha,
                Time.deltaTime * vignetteFadeSpeed
            );

            // ตั้งค่าสี: ดำสนิท (RGB=0) และ Alpha ตามที่คำนวณ
            sanityVignetteImage.color = new Color(0f, 0f, 0f, Mathf.Max(0f, currentVignetteAlpha));
        }
    }

    private void UpdateVignetteTarget(float currentSanity, float maxSanity)
    {
        // คำนวณค่า Alpha เป้าหมายจาก Sanity
        if (currentSanity > effectStartSanity)
        {
            // ถ้า Sanity สูงกว่าเกณฑ์ที่กำหนด (100f), ให้ Alpha เป้าหมายเป็น 0 
            targetVignetteAlpha = 0f;
        }
        else
        {
            // 1. คำนวณความรุนแรงเชิงเส้น (0 -> 1)
            float severity = 1f - (currentSanity / effectStartSanity);

            // 2. ใช้ Power Curve: (severity)^5.0 จะทำให้ค่าเริ่มต้นต่ำมาก แต่พุ่งเร็วตอนท้าย
            float curvedSeverity = Mathf.Pow(severity, fadeCurvePower);

            // 3. กำหนดค่า Target Alpha
            targetVignetteAlpha = Mathf.Clamp(curvedSeverity * MAX_FADE_ALPHA, 0f, MAX_FADE_ALPHA);
        }
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnSanityUpdate -= UpdateVignetteTarget;
        }
    }
}