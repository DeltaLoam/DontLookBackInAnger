using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class JumpscareTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("จะให้เกิด jumpscare ได้อีกไหมหลังเกิดครั้งแรก")]
    public bool oneTimeOnly = true;

    [Header("Scare Image")]
    public Image scareImage;
    public Sprite scareSprite;
    public float scareDuration = 2.5f;

    [Header("Scare Sound")]
    public AudioClip scareSound;

    [Header("Optional FX")]
    public ParticleSystem fxEffect;
    public Light flickerLight;

    private bool hasTriggered = false;
    private AudioSource audioSource;

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

    // ✅ ไม่เช็ค tag แล้ว
    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && oneTimeOnly) return;
        if (other.isTrigger) return; // กันไม่ให้ trigger ซ้อนกันเอง

        // ตัวอย่าง: ถ้าอยากให้เฉพาะวัตถุที่มี Rigidbody เท่านั้นถึงจะโดน
        // if (!other.attachedRigidbody) return;

        Debug.Log($"👻 Jumpscare Triggered by: {other.name}");
        TriggerScare();
    }

    void TriggerScare()
    {
        hasTriggered = true;

        if (scareImage != null && scareSprite != null)
        {
            scareImage.sprite = scareSprite;
            scareImage.enabled = true;
        }

        if (scareSound != null)
            audioSource.PlayOneShot(scareSound);

        if (fxEffect != null)
            fxEffect.Play();

        if (flickerLight != null)
            StartCoroutine(FlickerLight());

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
        var col = GetComponent<SphereCollider>();
        if (col != null)
            Gizmos.DrawWireSphere(transform.position, col.radius);
        else
            Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
