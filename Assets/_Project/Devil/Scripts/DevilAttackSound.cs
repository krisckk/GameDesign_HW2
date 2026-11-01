
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DevilAttackSound : MonoBehaviour
{
    [Header("Attack Sound")]
    public AudioClip attackSound;

    [Range(0f, 1f)]
    public float volume = 0.8f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, volume);
        }
    }
}
