using UnityEngine;

// ต้องแนบไปกับ GameObject ที่มี FirstPersonController และ PlayerStats
public class StaminaSync : MonoBehaviour
{
    private PlayerStats playerStats;
    // เราใช้ namespace ของ Controller เดิมเพื่อเข้าถึงมัน
    private EasyPeasyFirstPersonController.FirstPersonController controller;

    void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        controller = GetComponent<EasyPeasyFirstPersonController.FirstPersonController>();
        
        if (playerStats == null || controller == null)
        {
            Debug.LogError("StaminaSync requires PlayerStats and FirstPersonController on this GameObject.");
            enabled = false;
        }
    }

    void Update()
    {
        if (playerStats == null || controller == null) return;

        // ------------------------------------------------------
        // 1. SYNC Stamina (Drain and Regen)
        // เราใช้ field 'isSprinting' ที่เป็น public ของ Controller มาพิจารณา
        // ------------------------------------------------------
        bool isCurrentlySprinting = controller.isSprinting;
        
        if (isCurrentlySprinting) 
        {
            playerStats.UseStamina(playerStats.staminaDrainRate * Time.deltaTime);
        }
        else if (playerStats.CurrentStamina < playerStats.maxStamina)
        {
            playerStats.RecoverStamina(playerStats.regenRate * Time.deltaTime);
        }

        // ------------------------------------------------------
        // 2. BLOCK SPRINTING (โดยการควบคุม field 'canSprint' ของ Controller)
        // ------------------------------------------------------
        // ถ้าผู้เล่นหมดแรงหรือ Stamina ใกล้หมด ให้ Controller วิ่งไม่ได้
        if (playerStats.IsExhausted || playerStats.CurrentStamina < 1f)
        {
            // บังคับปิด canSprint ใน Controller
            controller.canSprint = false; 
        }
        else
        {
            // เปิดให้ Controller วิ่งได้ตามปกติ
            controller.canSprint = true;
        }
    }
}