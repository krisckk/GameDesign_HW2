
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MageAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;
    public Transform firePoint;
    public MagicProjectile projectilePrefab;

    [Header("Detection")]
    public float detectionRange = 15f;
    public float attackRange = 8f;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;

    [Header("Magic Attack")]
    public float attackCooldown = 2f;
    public float castDuration = 1.2f;
    public float fireDelay = 0.6f;

    [Header("Magic Sound")]
    public AudioSource audioSource;
    public AudioClip castSound;

    [Range(0.5f, 2f)]
    public float castSoundSpeed = 1.2f;

    [Range(0f, 1f)]
    public float castSoundVolume = 0.8f;

    private NavMeshAgent agent;
    private bool isCasting;
    private float nextAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.9f;
        agent.updateRotation = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("IsMoving", false);
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null || animator == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (isCasting)
        {
            FacePlayer();
            return;
        }

        Vector3 direction =
            player.position - transform.position;

        direction.y = 0f;
        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            StopMoving();
            animator.SetBool("IsMoving", false);
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();
            FacePlayer();
            animator.SetBool("IsMoving", false);

            if (Time.time >= nextAttackTime)
                StartCoroutine(CastMagic());

            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        animator.SetBool("IsMoving", true);

        Vector3 moveDirection = agent.desiredVelocity;

        if (moveDirection.sqrMagnitude > 0.01f)
            RotateTowards(moveDirection);
    }

    IEnumerator CastMagic()
    {
        isCasting = true;

        StopMoving();
        FacePlayer();

        animator.SetBool("IsMoving", false);
        animator.SetTrigger("Cast");

        float duration = Mathf.Max(0.1f, castDuration);
        float delay = Mathf.Clamp(fireDelay, 0f, duration);

        yield return new WaitForSeconds(delay);

        if (firePoint != null &&
            projectilePrefab != null &&
            player != null)
        {
            Vector3 targetPosition =
                player.position + Vector3.up * 1f;

            MagicProjectile projectile = Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

            projectile.Initialize(targetPosition, transform);

            if (audioSource != null && castSound != null)
            {
                audioSource.pitch = castSoundSpeed;
                audioSource.PlayOneShot(castSound, castSoundVolume);
            }
        }

        yield return new WaitForSeconds(duration - delay);

        nextAttackTime = Time.time + attackCooldown;
        isCasting = false;
    }

    void StopMoving()
    {
        if (!agent.isOnNavMesh)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position - transform.position;

        RotateTowards(direction);
    }

    void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
