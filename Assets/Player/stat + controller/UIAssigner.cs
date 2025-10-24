using UnityEngine;

public class UIAssigner : MonoBehaviour
{
    // ลาก Stamina_Fill (ที่มี StaminaBarUI.cs) มาใส่ใน Inspector
    [Header("UI References")]
    public StaminaBarUI staminaBarUI;

    // อ้างอิง PlayerStats บน GameObject เดียวกัน
    private PlayerStats playerStats;

    void Awake()
    {
        // รับ PlayerStats component
        playerStats = GetComponent<PlayerStats>();
    }

    void Start()
    {
        // ตรวจสอบว่ามี PlayerStats และได้ลาก UI มาใส่แล้ว
        if (playerStats != null && staminaBarUI != null)
        {
            // ⭐ นี่คือขั้นตอนสำคัญที่ต้องทำ
            staminaBarUI.SetTarget(playerStats);
            Debug.Log("UI successfully connected to PlayerStats.");
        }
        else
        {
            if (playerStats == null)
                Debug.LogError("UIAssigner: PlayerStats not found on this GameObject.");
            if (staminaBarUI == null)
                Debug.LogError("UIAssigner: Stamina Bar UI reference is missing. Please assign it in the Inspector.");
        }
    }
}