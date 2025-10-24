using UnityEngine;
using UnityEngine.UI;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("ระยะที่ผู้เล่นจะทำให้ jumpscare ทำงานได้")]
    public float triggerRadius = 5f;
    [Tooltip("จะให้เกิด jumpscare ได้อีกไหมหลังเกิดครั้งแรก")]
    public bool oneTimeOnly = true;

    [Header("Scare Image")]
    [Tooltip("UI Image ที่จะใช้แสดงภาพ jumpscare")]
    public Image scareImage;
    [Tooltip("ภาพที่จะใช้ตอน jumpscare")]
    public Sprite scareSprite;
    [Tooltip("ระยะเวลาที่ภาพจะโชว์")]
    public float scareDuration = 2.5f;

    [Header("Scare Sound")]
    public AudioClip scareSound;

    [Header("Optional FX")]
    public ParticleSystem fxEffect;
    public Light flickerLight;

    private bool hasTriggered = false;
    private AudioSource audioSource;
    private Transform player;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // เสียงเป็น 2D (เล่นจากจอ)
        }

        if (scareImage != null)
            scareImage.enabled = false;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (hasTriggered && oneTimeOnly) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= triggerRadius)
        {
            TriggerScare();
        }
    }

    void TriggerScare()
    {
        hasTriggered = true;

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

    System.Collections.IEnumerator FlickerLight()
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
