using UnityEngine;
using Fusion;
using System;

// สคริปต์นี้มีหน้าที่ติดตามและเข้าถึง Stamina ของ Local Player
public class StaminaSync : MonoBehaviour
{
    // ⭐ Reference to PlayerStats (จะถูกกำหนดค่าโดยอัตโนมัติผ่าน Event)
    [Header("Dependencies")]
    public PlayerStats playerStats;

    // --------------------------------------------------
    // Initialization & Event Handling
    // --------------------------------------------------

    void Awake()
    {
        // สมัครรับสัญญาณจาก Local Player Stats ทันที
        PlayerStats.OnLocalPlayerStatsReady += SetPlayerStats;
    }

    private void SetPlayerStats(PlayerStats stats)
    {
        // หากมีการเชื่อมต่อเก่า ให้ยกเลิกก่อน (ป้องกันการสมัครซ้ำ)
        if (playerStats != null)
        {
            // ถ้า StaminaSync มี Event ที่สมัครรับอยู่ ให้ยกเลิกที่นี่
        }

        // ตั้งค่า PlayerStats ใหม่ และยกเลิกการสมัครรับ Static Event
        playerStats = stats;
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;

        Debug.Log("StaminaSync: Successfully attached to Local Player Stats.");
    }


    // --------------------------------------------------
    // Core Logic (ตัวอย่างการใช้งาน Stamina)
    // --------------------------------------------------

    void Update()
    {
        // รัน Logic ที่เกี่ยวข้องกับการอ่านค่า Stamina ใน Update
        CheckAndLogStamina();
    }

    private void CheckAndLogStamina()
    {
        if (playerStats == null) return;

        // ⭐⭐⭐ แก้ไข CS1061: ใช้ CurrentStaminaReadOnly แทน CurrentStamina ⭐⭐⭐
        float currentStamina = playerStats.CurrentStaminaReadOnly;
        float maxStamina = playerStats.maxStamina;

        // ตัวอย่าง Logic การตรวจสอบค่า:
        if (currentStamina < maxStamina * 0.1f && !playerStats.IsExhausted)
        {
            // คุณอาจเพิ่ม Logic เช่น การแสดง Visual Warning ที่นี่
            // Debug.LogWarning("Stamina is critically low!");
        }

        if (playerStats.IsExhausted)
        {
            // Debug.Log("Player is exhausted!");
        }
    }

    // --------------------------------------------------
    // Cleanup
    // --------------------------------------------------

    void OnDestroy()
    {
        // ยกเลิกการสมัครรับ Event Static
        PlayerStats.OnLocalPlayerStatsReady -= SetPlayerStats;
    }
}