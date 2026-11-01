
using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 50;

    [Header("Animation")]
    public Animator animator;
    public string hitTrigger = "Hit";
    public string deathTrigger = "Die";

    [Header("Effects")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public AudioClip deathSound;
    public ParticleSystem hitEffect;

    [Header("Events")]
    public UnityEvent onDeath;

    public bool IsDead { get; private set; }

    private int currentHealth;

    void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0)
            return;

        currentHealth =
            Mathf.Max(0, currentHealth - amount);

        Debug.Log(
            gameObject.name + " HP: " + currentHealth
        );

        if (hitEffect != null)
            hitEffect.Play();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            PlayTrigger(hitTrigger);

            if (audioSource != null && hitSound != null)
                audioSource.PlayOneShot(hitSound);
        }
    }

    void Die()
    {
        IsDead = true;

        PlayTrigger(deathTrigger);

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        onDeath?.Invoke();
    }

    void PlayTrigger(string triggerName)
    {
        if (animator == null ||
            string.IsNullOrEmpty(triggerName))
            return;

        foreach (AnimatorControllerParameter parameter
                 in animator.parameters)
        {
            if (parameter.name == triggerName &&
                parameter.type == AnimatorControllerParameterType.Trigger)
            {
                animator.SetTrigger(triggerName);
                return;
            }
        }
    }
}
