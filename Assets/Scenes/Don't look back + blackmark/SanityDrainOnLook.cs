using UnityEngine;
using System.Collections.Generic;

public class SanityDrainOnLook : MonoBehaviour
{
    [Header("Drain Settings")]
    [Tooltip("อัตรา Sanity ที่จะลดต่อวินาที")]
    public float sanityDrainRate = 5f;

    [Header("Detection Settings")]
    [Tooltip("FoV (องศา) สำหรับตรวจจับการมองด้านหลัง (ค่าต่ำ = มุมแคบ/แม่นยำสูง)")]
    public float detectionAngle = 45f; // ⭐ ค่าเริ่มต้นถูกปรับให้แคบลงตามที่คุณต้องการ
    [Tooltip("ระยะทางสูงสุดที่ผู้เล่นจะตรวจจับได้")]
    public float maxDetectionDistance = 15f;

    // ⭐ ตัวแปรอ้างอิงของคนที่มอง (Looker)
    [Header("Looker References")]
    [Tooltip("PlayerStats ของผู้เล่นที่กำลังมอง (คนนี้)")]
    public PlayerStats myPlayerStats;
    [Tooltip("Transform ที่ใช้ในการมอง (มักจะเป็นกล้อง)")]
    public Transform lookTransform;

    void Update()
    {
        if (myPlayerStats == null || lookTransform == null) return;

        // 1. หาผู้เล่นคนอื่นๆ ทั้งหมดในฉาก (ต้องมี Tag เป็น "Player")
        GameObject[] allPlayers = GameObject.FindGameObjectsWithTag("Player");

        bool isLookingAtSomeone = false;

        foreach (GameObject player in allPlayers)
        {
            // ข้ามตัวเอง (ใช้ transform.root เพื่ออ้างถึง GameObject หลักของ Player)
            if (player.transform.root == transform.root) continue;

            // ตรวจสอบว่าผู้เล่นคนนี้กำลังมองหลังผู้เล่นคนอื่นอยู่หรือไม่
            if (IsLookingAtPlayersBack(player))
            {
                isLookingAtSomeone = true;
                break; // ลด Sanity แค่ครั้งเดียวต่อเฟรม
            }
        }

        // 2. ถ้ามองหลังใครบางคนอยู่ ให้ลด Sanity ของผู้ที่มอง (คนนี้)
        if (isLookingAtSomeone)
        {
            float drainAmount = sanityDrainRate * Time.deltaTime;
            myPlayerStats.ApplySanityDrain(drainAmount); // ใช้งานได้แล้วหลังแก้ Error CS1061
        }
    }

    // ⭐ Logic การตรวจจับ: ผู้มอง (Looker) มองหลัง (Black Mark) ของผู้ถูกมอง (Target)
    private bool IsLookingAtPlayersBack(GameObject targetPlayer)
    {
        // 1. หา BlackMark Transform
        Transform blackMarkTransform = targetPlayer.transform.Find("BlackMark");
        if (blackMarkTransform == null) return false;

        // 2. ตรวจสอบระยะทาง
        float distance = Vector3.Distance(lookTransform.position, targetPlayer.transform.position);
        if (distance > maxDetectionDistance) return false;

        // 3. ตรวจสอบ FoV ของผู้มอง (Looker)
        // LookerDot: ต้องเป็นค่าบวก หมายถึง Looker กำลังมองไปที่ BlackMark
        Vector3 directionToTarget = (blackMarkTransform.position - lookTransform.position).normalized;
        float lookerDot = Vector3.Dot(lookTransform.forward, directionToTarget);

        // ตรวจสอบว่า BlackMark อยู่ใน FoV ของผู้มองหรือไม่ (ใช้ detectionAngle)
        if (lookerDot < Mathf.Cos(detectionAngle / 2f * Mathf.Deg2Rad)) return false;


        // 4. ตรวจสอบการมองจากด้านหลัง Black Mark (Black Mark Condition)
        // BackMarkDot: ต้องเป็นค่าลบ หมายถึง Looker อยู่ด้านหลัง BlackMark
        Vector3 directionToLooker = (lookTransform.position - blackMarkTransform.position).normalized;
        float backMarkDot = Vector3.Dot(blackMarkTransform.forward, directionToLooker);

        // ถ้า backMarkDot > 0.1f หมายความว่ามองจากด้านหน้า/ด้านข้าง BlackMark
        if (backMarkDot > 0.1f) return false;


        // 5. Raycast ตรวจสอบสิ่งกีดขวาง
        if (Physics.Raycast(lookTransform.position, directionToTarget, out RaycastHit hit, distance))
        {
            // ตรวจสอบว่า Raycast ชนกับ BlackMark หรือ Player ของเราหรือไม่ (ใช้ transform.root)
            if (hit.transform.root != targetPlayer.transform.root)
            {
                return false; // ถูกบัง
            }
        }

        // ผ่านการตรวจสอบทั้งหมด: ผู้เล่นคนนี้กำลังมองหลังผู้เล่นคนอื่น
        return true;
    }
}