using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("จะให้เกิด jumpscare ได้อีกไหมหลังเกิดครั้งแรก")]
    public bool oneTimeOnly = true;

    [Tooltip("ระยะที่ตรวจว่ามีคนอยู่ใกล้เพื่อ jumpscare")]
    public float triggerRadius = 5f;

    [Tooltip("เวลาหน่วงก่อน jumpscare จะทำงานจริง")]
    public float scareDelay = 2.0f; // 🔹 ปรับได้จาก Inspector

    [Header("Scare Image")]
    public Image scareImage;
    public Sprite scareSprite;
    public float scareDuration = 2.5f;

    [Header("Scare Sound")]
    public AudioClip scareSound;

    [Header("Optional FX")]
    public ParticleSystem fxEffect;
    public Light flickerLight;

    [Header("Sanity Effect")]
    [Tooltip("จำนวน Sanity ที่จะลดเมื่อโดน jumpscare")]
    public float sanityDrainAmount = 25f;

    private bool hasTriggered = false;
    private AudioSource audioSource;
    private Coroutine scareCoroutine; // สำหรับจัดการ Delay

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // เสียง 2D
        }

        if (scareImage != null)
            scareImage.enabled = false;
    }

    void Update()
    {
        if (hasTriggered && oneTimeOnly) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius);
        foreach (Collider hit in hits)
        {
            PlayerStats stats = hit.GetComponent<PlayerStats>();
            if (stats != null)
            {
                // 🔹 เริ่ม coroutine รอ delay ก่อน jumpscare
                if (scareCoroutine == null)
                    scareCoroutine = StartCoroutine(DelayedScare(stats));
                return;
            }
        }

        // 🔹 ถ้าไม่มีใครอยู่ในระยะ ให้ยกเลิกการนับถอยหลัง
        if (scareCoroutine != null)
        {
            StopCoroutine(scareCoroutine);
            scareCoroutine = null;
        }
    }

    IEnumerator DelayedScare(PlayerStats targetStats)
    {
        float timer = 0f;

        // 🔸 รอจนถึงเวลาที่กำหนด scareDelay
        while (timer < scareDelay)
        {
            timer += Time.deltaTime;

            // ถ้าออกจากระยะก่อนครบเวลา → ยกเลิก
            if (Vector3.Distance(transform.position, targetStats.transform.position) > triggerRadius)
                yield break;

            yield return null;
        }

        // ครบเวลา → jumpscare ทำงาน
        TriggerScare(targetStats);
    }

    void TriggerScare(PlayerStats targetStats)
    {
        hasTriggered = true;

        // 🔹 ลด Sanity
        if (targetStats != null)
        {
            targetStats.ApplySanityDrain(sanityDrainAmount);
            Debug.Log($"😱 Jumpscare! {targetStats.name} lost {sanityDrainAmount} sanity.");
        }

        // 🔹 แสดงภาพ jumpscare
        if (scareImage != null && scareSprite != null)
        {
            scareImage.sprite = scareSprite;
            scareImage.enabled = true;
        }

        // 🔊 เล่นเสียง
        if (scareSound != null)
            audioSource.PlayOneShot(scareSound);

        // ✨ เอฟเฟกต์เสริม
        if (fxEffect != null)
            fxEffect.Play();

        if (flickerLight != null)
            StartCoroutine(FlickerLight());

        // 🔻 ปิดภาพหลังครบเวลา
        Invoke(nameof(EndScare), scareDuration);
    }

    void EndScare()
    {
        if (scareImage != null)
            scareImage.enabled = false;
    }

    IEnumerator FlickerLight()
    {
        float endTime = Time.time + scareDuration;
        while (Time.time < endTime)
        {
            flickerLight.enabled = !flickerLight.enabled;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
        }
        flickerLight.enabled = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
