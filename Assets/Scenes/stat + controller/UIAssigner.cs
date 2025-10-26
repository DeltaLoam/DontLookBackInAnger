using UnityEngine;
using System;
using Fusion; // สำหรับบริบท Network

public class UIAssigner : MonoBehaviour
{
    // ไม่ต้องลากใส่ Inspector แล้ว: StaminaBarUI จะถูกค้นหาเอง
    [Header("UI References")]
    private StaminaBarUI staminaBarUI;

    private PlayerStats localPlayerStats;

    void Awake()
    {
        // ⭐ ค้นหา StaminaBarUI ใน Scene โดยอัตโนมัติ (แก้ปัญหาไม่ต้องลากใส่)
        staminaBarUI = FindObjectOfType<StaminaBarUI>();

        // สมัครรับสัญญาณจาก Local Player Stats
        PlayerStats.OnLocalPlayerStatsReady += ReceivePlayerStats;
    }

    private void ReceivePlayerStats(PlayerStats stats)
    {
        if (localPlayerStats == null)
        {
            localPlayerStats = stats;
            // เรียก Logic การเชื่อมต่อทันทีที่ Local Stats พร้อม
            StartLogic();

            // ยกเลิกการสมัครรับ Event
            PlayerStats.OnLocalPlayerStatsReady -= ReceivePlayerStats;
        }
    }

    void StartLogic()
    {
        // Logic การ Assign UI
        if (localPlayerStats != null && staminaBarUI != null)
        {
            // ⭐⭐ แก้ไข Error CS1061 โดยการเรียกเมธอด SetTarget ที่ถูกเพิ่มเข้าไป ⭐⭐
            staminaBarUI.SetTarget(localPlayerStats);
            Debug.Log("UIAssigner: UI successfully connected to Local Player Stats via FindObjectOfType.");
        }
        else if (staminaBarUI == null)
        {
            Debug.LogError("UIAssigner failed: StaminaBarUI not found in the Scene (FindObjectOfType failed).");
        }
        else
        {
            Debug.LogError("UIAssigner failed: Local Player Stats were not received.");
        }
    }

    void OnDestroy()
    {
        // ยกเลิกการสมัครรับ Event เผื่อไว้
        PlayerStats.OnLocalPlayerStatsReady -= ReceivePlayerStats;
    }
}