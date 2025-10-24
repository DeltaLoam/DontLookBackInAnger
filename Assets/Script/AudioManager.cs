using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource ambientSource;

    [Header("Jumpscare Clips")]
    public AudioClip[] scareClips;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public AudioClip GetRandomScareClip()
    {
        if (scareClips == null || scareClips.Length == 0) return null;
        return scareClips[Random.Range(0, scareClips.Length)];
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (ambientSource == null) return;
        ambientSource.clip = clip;
        ambientSource.loop = loop;
        ambientSource.Play();
    }
}
