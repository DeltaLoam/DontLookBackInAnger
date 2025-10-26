using UnityEngine;
using UnityEngine.UI; 
using System;

public class SanityVisuals : MonoBehaviour
{
    // PlayerStats จะถูกกำหนดค่าโดยอัตโนมัติผ่าน Event
    [Header("Dependencies (Auto-Assigned)")]
    private PlayerStats playerStats; 

    [Header("UI References")]
    // RawImage ต้องเป็น PRIVATE เพื่อไม่ให้มีช่องใน Inspector และค้นหาเอง
    private RawImage sanityVignetteImage; 
    
    // Texture ถูกโหลดอัตโนมัติจาก Resources Folder
    private Texture sanityEffectTexture; 
    
    private const string VIGNETTE_IMAGE_NAME = "Sanity_Vignette_Raw";
    // ⭐ ชื่อไฟล์ Texture ที่ใช้โหลดอัตโนมัติจาก Resources Folder ⭐
    private const string VIGNETTE_TEXTURE_NAME = "download (17)";

    [Header("Visual Settings")]
    public float maxAlpha = 0.8f; 
    public float fadeSpeed = 5f; 
    
    void Awake()
    {
        // 1. ลองโหลด Texture อัตโนมัติจาก Resources Folder
        // NOTE: ไฟล์ 'download (17)' ต้องอยู่ในโฟลเดอร์ชื่อ 'Resources'
        sanityEffectTexture = Resources.Load<Texture>(VIGNETTE_TEXTURE_NAME);

        // 2. สมัครรับ Static Event เพื่อรอ PlayerStats (ใช้เป็นตัวจุดชนวน)
        PlayerStats.OnLocalPlayerStatsReady += SetPlayerStats;
    }
    
    private void SetPlayerStats(PlayerStats stats)
    {
        // Unsubscribe จาก Event เก่า
        if (playerStats != null) 
        { 
            playerStats.OnSanityUpdate -= UpdateVignette; 
        }
        
        playerStats = stats;
        
        // ⭐ เรียก SetupVisuals ทันทีที่ PlayerStats พร้อม ⭐
        SetupVisuals(playerStats);
        
        // ยกเลิกการสมัครรับ Event Static
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }

    private void SetupVisuals(PlayerStats stats)
    {
        // ⭐ NEW: ค้นหา RawImage เมื่อ PlayerStats ถูกกำหนดค่าแล้ว ⭐
        GameObject rawImageGO = GameObject.Find(VIGNETTE_IMAGE_NAME);
        if (rawImageGO != null) 
        { 
            sanityVignetteImage = rawImageGO.GetComponent<RawImage>(); 
        }

        // ⭐⭐⭐ LOGGING ตรวจสอบว่ามีตัวใดเป็น null บ้าง ⭐⭐⭐
        bool statsMissing = (stats == null);
        bool rawImageMissing = (sanityVignetteImage == null);
        bool textureMissing = (sanityEffectTexture == null);
        
        if (statsMissing || rawImageMissing || textureMissing)
        {
            string missingRefs = "";
            if (statsMissing) missingRefs += "PlayerStats. ";
            if (rawImageMissing) missingRefs += $"RawImage ('{VIGNETTE_IMAGE_NAME}' not found/missing component). ";
            if (textureMissing) missingRefs += $"Texture ('{VIGNETTE_TEXTURE_NAME}' not found in Resources folder!).";
            
            Debug.LogError($"[FATAL SanityVisuals] Setup Failed! Missing: {missingRefs}");
            Debug.LogError("SanityVisuals setup failed. Check references: PlayerStats, RawImage, and Texture must be assigned.");
            return;
        }

        // Logic การตั้งค่าเริ่มต้น
        sanityVignetteImage.texture = sanityEffectTexture;
        sanityVignetteImage.color = new Color(1f, 1f, 1f, 0f); 
        
        stats.OnSanityUpdate += UpdateVignette;
        UpdateVignette(stats.CurrentSanity, stats.maxSanity);
    }
    
    private void UpdateVignette(float currentSanity, float maxSanity)
    {
        if (sanityVignetteImage == null) return;

        float sanityNormalized = 1f - (currentSanity / maxSanity);
        float targetAlpha = sanityNormalized * maxAlpha;
        
        Color targetColor = sanityVignetteImage.color;
        targetColor.a = targetAlpha;

        // ใช้ Time.deltaTime ในการทำให้การเปลี่ยนแปลงราบรื่น
        sanityVignetteImage.color = Color.Lerp(sanityVignetteImage.color, targetColor, Time.deltaTime * fadeSpeed); 
    }

    void OnDestroy()
    {
        if (playerStats != null)
        {
            playerStats.OnSanityUpdate -= UpdateVignette;
        }
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }
}