using UnityEngine;
using TMPro; // ⭐ เปลี่ยนมาใช้ TMPro แทน UnityEngine.UI
using System;

public class LifeDisplayUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("อ้างอิงถึง PlayerStats ของผู้เล่นที่เราต้องการแสดงผล")]
    public PlayerStats playerStats;

    [Tooltip("องค์ประกอบ UI TextMeshPro ที่จะแสดงจำนวนชีวิต")]
    public TextMeshProUGUI lifeText; // ⭐ เปลี่ยนชนิดตัวแปรเป็น TextMeshProUGUI

    private void OnEnable()
    {
        // สมัครรับ Event เมื่อสคริปต์เปิดใช้งาน
        if (playerStats != null)
        {
            playerStats.OnLifeUpdate += UpdateLifeDisplay;
            playerStats.OnGameOver += HandleGameOverDisplay;
        }
    }

    private void OnDisable()
    {
        // ยกเลิกการสมัครรับ Event เมื่อสคริปต์ปิดใช้งานเพื่อป้องกัน Error
        if (playerStats != null)
        {
            playerStats.OnLifeUpdate -= UpdateLifeDisplay;
            playerStats.OnGameOver -= HandleGameOverDisplay;
        }
    }

    private void Start()
    {
        // ตั้งค่าเริ่มต้นของ UI เมื่อเกมเริ่ม
        if (playerStats != null && lifeText != null)
        {
            UpdateLifeDisplay(playerStats.CurrentLife);
        }
        else
        {
            Debug.LogError("LifeDisplayUI: ต้องกำหนดค่า PlayerStats และ UI TextMeshPro ใน Inspector!");
        }
    }

    /// <summary>
    /// ถูกเรียกทุกครั้งที่จำนวนชีวิตมีการเปลี่ยนแปลง
    /// </summary>
    /// <param name="currentLives">จำนวนชีวิตที่เหลือ</param>
    private void UpdateLifeDisplay(int currentLives)
    {
        if (lifeText != null)
        {
            // แสดงจำนวนชีวิตที่เหลือในรูปแบบข้อความ
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
            // คุณอาจต้องการแสดง UI Game Over อื่นๆ ตรงนี้
        }
    }
}