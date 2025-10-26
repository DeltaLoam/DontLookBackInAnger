using UnityEngine;
using TMPro; // ใช้ TMPro
using System;

public class LifeDisplayUI : MonoBehaviour
{
    [Header("References")]
    // เปลี่ยนเป็น private เพื่อไม่ให้ลากใส่ใน Inspector (ถูกกำหนดค่าอัตโนมัติ)
    private PlayerStats playerStats;

    [Tooltip("องค์ประกอบ UI TextMeshPro ที่จะแสดงจำนวนชีวิต")]
    public TextMeshProUGUI lifeText; 

    void Awake()
    {
        // 1. สมัครรับ Static Event ใน Awake เพื่อรอ PlayerStats ของ Local Player
        PlayerStats.OnLocalPlayerStatsReady += SetPlayerStats;

        // ตรวจสอบ Text Component ทันที (ถ้ายังไม่ได้กำหนดค่า จะแสดง Error)
        if (lifeText == null)
        {
            Debug.LogError("LifeDisplayUI: ต้องกำหนดค่า UI TextMeshPro ใน Inspector!");
        }
    }

    private void OnDisable()
    {
        // ยกเลิกการสมัครรับ Event ของ PlayerStats ตัวเก่า (ถ้ามี)
        if (playerStats != null)
        {
            playerStats.OnLifeUpdate -= UpdateLifeDisplay;
            playerStats.OnGameOver -= HandleGameOverDisplay;
        }
        // ยกเลิกการสมัครรับ Static Event
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }

    /// <summary>
    /// ถูกเรียกเมื่อ PlayerStats ของ Local Player พร้อมใช้งาน
    /// </summary>
    /// <param name="stats">Reference ถึง PlayerStats ที่ถูกต้อง</param>
    private void SetPlayerStats(PlayerStats stats)
    {
        // ยกเลิกการสมัคร Event ตัวเก่าก่อน
        if (playerStats != null)
        {
            playerStats.OnLifeUpdate -= UpdateLifeDisplay;
            playerStats.OnGameOver -= HandleGameOverDisplay;
        }

        // กำหนดค่า PlayerStats ที่ถูกต้อง
        playerStats = stats;
        
        // 2. สมัครรับ Event ของ PlayerStats ตัวใหม่
        playerStats.OnLifeUpdate += UpdateLifeDisplay;
        playerStats.OnGameOver += HandleGameOverDisplay;

        // 3. ตั้งค่าและอัปเดตค่าเริ่มต้นทันที
        UpdateLifeDisplay(playerStats.CurrentLife);
    }


    /// <summary>
    /// ถูกเรียกทุกครั้งที่จำนวนชีวิตมีการเปลี่ยนแปลง
    /// </summary>
    /// <param name="currentLives">จำนวนชีวิตที่เหลือ</param>
    private void UpdateLifeDisplay(int currentLives)
    {
        if (lifeText != null)
        {
            lifeText.text = $"{currentLives}";
        }
    }

    /// <summary>
    /// ถูกเรียกเมื่อผู้เล่นตายและชีวิตเหลือ 0 (Game Over)
    /// </summary>
    private void HandleGameOverDisplay()
    {
        if (lifeText != null)
        {
            lifeText.text = "GAME OVER";
        }
    }
}