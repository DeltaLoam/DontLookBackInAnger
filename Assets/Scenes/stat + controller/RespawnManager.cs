using UnityEngine;
using System.Collections.Generic;
using System.Linq; // ต้องมีสำหรับการใช้ Linq (OrderBy)

public class RespawnManager : MonoBehaviour
{
    // ⭐ Singleton Pattern (เพื่อให้เข้าถึงได้ง่ายจากที่อื่น)
    public static RespawnManager Instance;

    // List ของจุดเกิดทั้งหมดในฉาก
    private List<Transform> respawnPoints = new List<Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 1. ค้นหา GameObject ทั้งหมดที่มี Tag "RespawnPoint"
        GameObject[] points = GameObject.FindGameObjectsWithTag("Respawn");

        // 2. แปลงเป็น Transform และเพิ่มเข้าไปใน List
        respawnPoints.AddRange(points.Select(p => p.transform));

        // ⭐ (Optional) จัดเรียงจุดเกิดตามชื่อเพื่อความง่ายในการดีบั๊ก
        respawnPoints = respawnPoints.OrderBy(t => t.name).ToList();

        Debug.Log($"RespawnManager: Found {respawnPoints.Count} respawn points.");
    }

    /// <summary>
    /// เมธอดสำหรับดึงจุดเกิดใหม่
    /// (ในตัวอย่างนี้ เราจะใช้จุดแรกสุดเป็นจุดเกิดเริ่มต้น)
    /// </summary>
    /// <returns>Transform ของจุดเกิดที่ถูกเลือก</returns>
    public Vector3 GetRespawnPosition(int pointIndex = 0)
    {
        if (respawnPoints.Count == 0)
        {
            Debug.LogError("No respawn points found! Returning origin (0,0,0).");
            return Vector3.zero;
        }

        // ถ้า index เกินขอบเขต ให้กลับไปใช้จุดแรกสุด
        if (pointIndex >= respawnPoints.Count)
        {
            pointIndex = 0;
        }

        return respawnPoints[pointIndex].position;
    }
}