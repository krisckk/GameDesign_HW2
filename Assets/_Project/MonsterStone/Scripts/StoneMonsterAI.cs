
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
public class StoneMonsterAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float chaseRange = 8f;
    public float attackRange = 2.3f;
    public float moveSpeed = 120f;
    public float attackCooldown = 2.7f;

    [Header("Animation")]
    public Animation monsterAnimation;

    [Header("Attack Sound")]
    public AudioClip biteSound;
    public AudioSource audioSource;
    public float biteSoundDelay = 0.3f;

    [Header("Bite Effect")]
    public ParticleSystem biteEffect;

    private NavMeshAgent agent;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        agent.stoppingDistance = attackRange * 0.8f;

        if (monsterAnimation == null)
            monsterAnimation =
                GetComponentInChildren<Animation>();

        if (player == null)
        {
            GameObject obj =
                GameObject.FindGameObjectWithTag("Player");

            if (obj != null)
                player = obj.transform;
        }

        PlayAnimation("Anim_Idle");

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh)
            return;

        float distance = Vector3.Distance(
            transform.position,
            player.position
        );

        if (isAttacking)
            return;

        if (distance <= attackRange)
        {
            agent.isStopped = true;
            agent.ResetPath();

            FacePlayer();

            if (Time.time >= lastAttackTime + attackCooldown)
                StartCoroutine(Attack());
            else
                PlayAnimation("Anim_Idle");
        }
        else if (distance <= chaseRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            PlayAnimation("Anim_Run");
        }
        else
        {
            agent.isStopped = true;
            agent.ResetPath();

            PlayAnimation("Anim_Idle");
        }
    }

    
    IEnumerator Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        agent.isStopped = true;
        agent.ResetPath();

        FacePlayer();

        PlayAnimation("Anim_Attack");

        float duration = 1f;

        if (monsterAnimation != null &&
            monsterAnimation["Anim_Attack"] != null)
        {
            duration = monsterAnimation["Anim_Attack"].length;
        }

        float delay = Mathf.Clamp(
            biteSoundDelay, 0f, duration
        );

        yield return new WaitForSeconds(delay);

        if (audioSource != null && biteSound != null)
        {
            audioSource.pitch = 1.2f;
            audioSource.PlayOneShot(biteSound);
        }

        if (biteEffect != null)
        {
            biteEffect.Stop(true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            biteEffect.Play();
        }

        yield return new WaitForSeconds(duration - delay);

        isAttacking = false;
    }


    void FacePlayer()
    {
        Vector3 direction =
            player.position - transform.position;

        direction.y = 0;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion rotation =
                Quaternion.LookRotation(direction);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotation,
                Time.deltaTime * 8f
            );
        }
    }

    void PlayAnimation(string animationName)
    {
        if (monsterAnimation == null)
            return;

        if (monsterAnimation[animationName] == null)
            return;

        if (!monsterAnimation.IsPlaying(animationName))
        {
            monsterAnimation.CrossFade(animationName, 0.15f);
        }
    }
}
