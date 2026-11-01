
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animator animator;

    [Header("Detection")]
    public float detectionRange = 10f;
    public float attackRange = 2f;

    [Header("Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public float attackCooldown = 0.5f;

    private NavMeshAgent agent;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("IsFlying", false);
            animator.SetBool("IsAttacking", false);
        }
    }

    void Update()
    {
        if (player == null || animator == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (isAttacking)
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
            animator.SetBool("IsFlying", false);
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();
            FacePlayer();

            animator.SetBool("IsFlying", false);

            if (Time.time >= nextAttackTime)
            {
                StartCoroutine(PerformAttack());
            }

            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        animator.SetBool("IsFlying", true);
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        StopMoving();

        animator.SetBool("IsFlying", false);
        animator.SetBool("IsAttacking", true);

        while (!animator.GetCurrentAnimatorStateInfo(0)
                        .IsName("attack_1"))
        {
            yield return null;
        }

        while (animator.GetCurrentAnimatorStateInfo(0)
                       .normalizedTime < 1f)
        {
            yield return null;
        }

        animator.SetBool("IsAttacking", false);

        isAttacking = false;

        nextAttackTime = Time.time + attackCooldown;
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
        Vector3 direction =
            player.position - transform.position;

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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}
