
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StoneMonsterAI : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Animation monsterAnimation;
    public AudioSource audioSource;
    public AudioClip biteSound;
    public ParticleSystem biteEffect;

    [Header("Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1.8f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public int attackDamage = 10;
    public float attackCooldown = 1.2f;
    public float biteDelay = 0.35f;

    private NavMeshAgent agent;
    private PlayerHealth playerHealth;
    private bool isAttacking;
    private bool isDead;
    private float nextAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;
        agent.updateRotation = false;

        if (monsterAnimation == null)
            monsterAnimation = GetComponentInChildren<Animation>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject target =
                GameObject.FindGameObjectWithTag("Player");

            if (target != null)
                player = target.transform;
        }

        if (player != null)
            playerHealth = player.GetComponentInParent<PlayerHealth>();

        if (monsterAnimation != null)
        {
            SetLoop("Anim_Idle", WrapMode.Loop);
            SetLoop("Anim_Run", WrapMode.Loop);

            SetLoop("Anim_Attack", WrapMode.Once);
            SetLoop("Anim_Damage", WrapMode.Once);
            SetLoop("Anim_Death", WrapMode.Once);

            PlayAnimation("Anim_Idle");
        }

        if (biteEffect != null)
            biteEffect.Stop(true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        if (isDead || player == null || monsterAnimation == null)
            return;

        if (!agent.isOnNavMesh)
            return;

        if (isAttacking)
        {
            FacePlayer();
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > detectionRange)
        {
            StopMoving();
            PlayAnimation("Anim_Idle");
            return;
        }

        if (distance <= attackRange)
        {
            StopMoving();
            FacePlayer();

            if (Time.time >= nextAttackTime)
                StartCoroutine(PerformAttack());
            else
                PlayAnimation("Anim_Idle");

            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        if (agent.desiredVelocity.sqrMagnitude > 0.01f)
            RotateTowards(agent.desiredVelocity);

        PlayAnimation("Anim_Run");
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        StopMoving();
        FacePlayer();

        PlayAnimation("Anim_Attack");

        float duration = GetAnimationDuration("Anim_Attack");
        float delay = Mathf.Clamp(biteDelay, 0f, duration);

        yield return new WaitForSeconds(delay);

        if (audioSource != null && biteSound != null)
            audioSource.PlayOneShot(biteSound);

        if (biteEffect != null)
        {
            biteEffect.Stop(true,
                ParticleSystemStopBehavior.StopEmittingAndClear);
            biteEffect.Play();
        }

        if (player != null && playerHealth != null)
        {
            Vector3 direction = player.position - transform.position;
            direction.y = 0f;

            if (direction.magnitude <= attackRange)
                playerHealth.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(duration - delay);

        isAttacking = false;
        nextAttackTime = Time.time + attackCooldown;
    }

    void PlayAnimation(string animationName)
    {
        if (monsterAnimation == null ||
            monsterAnimation[animationName] == null)
            return;

        if (!monsterAnimation.IsPlaying(animationName))
            monsterAnimation.CrossFade(animationName, 0.1f);
    }

    void SetLoop(string name, WrapMode mode)
    {
        if (monsterAnimation[name] != null)
            monsterAnimation[name].wrapMode = mode;
    }

    float GetAnimationDuration(string name)
    {
        if (monsterAnimation[name] == null)
            return 1f;

        return monsterAnimation[name].length /
            Mathf.Max(0.01f, monsterAnimation[name].speed);
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
        if (player == null) return;

        RotateTowards(player.position - transform.position);
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

    public void StopAI()
    {
        isDead = true;
        StopAllCoroutines();

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        enabled = false;
    }
}
