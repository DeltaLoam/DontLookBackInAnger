using UnityEngine;

public class GhostOrb : MonoBehaviour
{
    [Header("Settings")]
    public int orbID;
    public bool isCollected = false;
    public float rotateSpeed = 50f;

    [Header("Effects")]
    public AudioClip collectSound;
    

    private void Update()
    {
        // หมุน orb เบา ๆ เพื่อให้ดูมีชีวิต
        if (!isCollected)
            transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // ✅ ตรวจว่า GameObject ที่ชนมี Tag "Player"
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
{
    if (isCollected) return;
    isCollected = true;

    // 🔊 เล่นเสียงตอนเก็บ orb
    if (collectSound)
        AudioSource.PlayClipAtPoint(collectSound, transform.position);

    // 🧮 แจ้ง GhostOrbManager ให้เพิ่มจำนวน orb ที่เก็บได้
    if (GhostOrbManager.Instance != null)
        GhostOrbManager.Instance.AddOrb();

    // 🔹 ทำลายหรือซ่อน orb
    Destroy(gameObject);
}

    
    
}
